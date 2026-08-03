using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Observability;
using SmartInsure.Infra.Data.Observability;
using SmartInsure.Infra.Data.Options;

namespace SmartInsure.Tests.Infra.Data.Observability;

/// <summary>
/// ADR-102 — recorder do log de integração da Cotação PlugV2: truncamento de 256 KB por lado, retenção
/// (ExpiresAtUtc via TTL) e best-effort (falha ao gravar nunca sobe para o chamador).
/// </summary>
public class QuotationIntegrationLogRecorderTests
{
    private readonly IMongoRepository<QuotationIntegrationLog> _repository =
        Substitute.For<IMongoRepository<QuotationIntegrationLog>>();

    private readonly ILogger<QuotationIntegrationLogRecorder> _logger =
        Substitute.For<ILogger<QuotationIntegrationLogRecorder>>();

    private QuotationIntegrationLogRecorder BuildRecorder(int retentionDays = 30)
        => new(
            _repository,
            Options.Create(new MongoOptions
            {
                ConnectionString = "mongodb://localhost:27017",
                Database = "SmartInsureTests",
                IntegrationLogRetentionDays = retentionDays,
            }),
            _logger);

    private static QuotationIntegrationLogContext BuildContext(string requestPayload, string? responseRaw = "{}") => new()
    {
        QuotationId = Guid.CreateVersion7(),
        QuotationGroupId = Guid.CreateVersion7(),
        InsurerId = Guid.CreateVersion7(),
        EngineType = "PlugV2",
        Outcome = QuotationIntegrationOutcome.Completed,
        QuotationStatus = "ReadyForEmission",
        DurationMs = 123,
        RequestPayload = requestPayload,
        ResponseRaw = responseRaw,
        HttpStatus = 200,
        ErrorMessage = null,
    };

    [Fact]
    [Trait("RuleId", "ADR-102")]
    public async Task RecordCotationAsync_TruncaPayload_QuandoExcede256KB()
    {
        var recorder = BuildRecorder();
        // 300_000 chars > 262_144 (256 KB) em ambos os lados (request e response).
        var oversizedRequest = new string('a', 300_000);
        var oversizedResponse = new string('b', 300_000);

        QuotationIntegrationLog? captured = null;
        _repository.InsertAsync(Arg.Do<QuotationIntegrationLog>(document => captured = document), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        await recorder.RecordCotationAsync(BuildContext(oversizedRequest, oversizedResponse), CancellationToken.None);

        captured.Should().NotBeNull();
        captured!.Request.Truncated.Should().BeTrue();
        captured.Request.Payload.Length.Should().Be(262_144);
        captured.Response.Truncated.Should().BeTrue();
        captured.Response.Raw.Length.Should().Be(262_144);
    }

    [Fact]
    [Trait("RuleId", "ADR-102")]
    public async Task RecordCotationAsync_NaoTrunca_QuandoPayloadDentroDoLimite()
    {
        var recorder = BuildRecorder();

        QuotationIntegrationLog? captured = null;
        _repository.InsertAsync(Arg.Do<QuotationIntegrationLog>(document => captured = document), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        await recorder.RecordCotationAsync(BuildContext("{\"ok\":true}"), CancellationToken.None);

        captured.Should().NotBeNull();
        captured!.Request.Truncated.Should().BeFalse();
        captured.Response.Truncated.Should().BeFalse();
    }

    [Fact]
    [Trait("RuleId", "ADR-102")]
    public async Task RecordCotationAsync_CalculaExpiresAtUtc_ComARetencaoConfigurada()
    {
        const int retentionDays = 7;
        var recorder = BuildRecorder(retentionDays);

        QuotationIntegrationLog? captured = null;
        _repository.InsertAsync(Arg.Do<QuotationIntegrationLog>(document => captured = document), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        await recorder.RecordCotationAsync(BuildContext("{}"), CancellationToken.None);

        captured.Should().NotBeNull();
        (captured!.ExpiresAtUtc - captured.CreatedAtUtc).Should().Be(TimeSpan.FromDays(retentionDays));
    }

    [Fact]
    [Trait("RuleId", "ADR-102")]
    public async Task RecordCotationAsync_NaoPropaga_QuandoInsertAsyncLanca()
    {
        // Best-effort (ADR-102): a Cotação nunca pode depender do log de integração.
        var recorder = BuildRecorder();
        _repository.InsertAsync(Arg.Any<QuotationIntegrationLog>(), Arg.Any<CancellationToken>())
            .Throws(new InvalidOperationException("Mongo indisponível."));

        var act = () => recorder.RecordCotationAsync(BuildContext("{}"), CancellationToken.None);

        await act.Should().NotThrowAsync();
    }
}

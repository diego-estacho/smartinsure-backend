using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Observability;
using SmartInsure.Infra.Data.Options;

namespace SmartInsure.Infra.Data.Observability;

/// <summary>
/// Implementação real do log de integração da Cotação PlugV2 (ADR-102) — primeiro consumidor efetivo do
/// IMongoRepository&lt;&gt;. Monta o documento (truncamento de 256 KB por lado, TTL via
/// Mongo:IntegrationLogRetentionDays, CorrelationId do Activity corrente) e grava best-effort: qualquer
/// falha na gravação é engolida (warning) — a Cotação nunca pode depender do log.
/// </summary>
public sealed class QuotationIntegrationLogRecorder(
    IMongoRepository<QuotationIntegrationLog> repository,
    IOptions<MongoOptions> options,
    ILogger<QuotationIntegrationLogRecorder> logger) : IQuotationIntegrationLogRecorder
{
    /// <summary>256 KB (ADR-102) — teto de cada payload (request/response) gravado.</summary>
    private const int MaxPayloadChars = 256 * 1024;

    public async Task RecordCotationAsync(QuotationIntegrationLogContext context, CancellationToken cancellationToken)
    {
        try
        {
            var createdAtUtc = DateTime.UtcNow;
            var (requestPayload, requestTruncated) = Truncate(context.RequestPayload);
            var (responseRaw, responseTruncated) = Truncate(context.ResponseRaw ?? string.Empty);

            var document = new QuotationIntegrationLog
            {
                QuotationId = context.QuotationId,
                QuotationGroupId = context.QuotationGroupId,
                InsurerId = context.InsurerId,
                EngineType = context.EngineType,
                Outcome = context.Outcome,
                QuotationStatus = context.QuotationStatus,
                CreatedAtUtc = createdAtUtc,
                ExpiresAtUtc = createdAtUtc.AddDays(options.Value.IntegrationLogRetentionDays),
                DurationMs = context.DurationMs,
                CorrelationId = Activity.Current?.Id,
                Request = new QuotationIntegrationLogPayload
                {
                    Payload = requestPayload,
                    Truncated = requestTruncated,
                },
                Response = new QuotationIntegrationLogResponse
                {
                    Raw = responseRaw,
                    HttpStatus = context.HttpStatus,
                    Truncated = responseTruncated,
                },
                ErrorMessage = context.ErrorMessage,
            };

            await repository.InsertAsync(document, cancellationToken);
        }
        catch (Exception exception)
        {
            // Best-effort (ADR-102): a Cotação nunca pode depender do log de integração.
            logger.LogWarning(
                exception,
                "Falha ao gravar o log de integração da Cotação {QuotationId} (Grupo {QuotationGroupId}) no Mongo.",
                context.QuotationId,
                context.QuotationGroupId);
        }
    }

    private static (string Value, bool Truncated) Truncate(string value)
        => value.Length <= MaxPayloadChars ? (value, false) : (value[..MaxPayloadChars], true);
}

using FluentAssertions;
using NSubstitute;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.GetQuotationDetail;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.GetQuotationDetail.Requests;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.GetQuotationDetail.Responses;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Repositories.Dtos;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;
using Xunit;

namespace SmartInsure.Tests.Application.UseCases.QuotationUseCases.GetQuotationDetail;

/// <summary>RN-081 — detalhe read-only da Cotação: Escopo ativo (404 fora), mapeamento e cronologia mínima.</summary>
[Trait("Category", "UseCase")]
[Trait("RuleId", "RN-081")]
public sealed class GetQuotationDetailUseCaseTests
{
    private readonly IQuotationRepository _quotationRepository = Substitute.For<IQuotationRepository>();
    private readonly GetQuotationDetailUseCase _useCase;

    private readonly Guid _brokerageId = Guid.NewGuid();
    private readonly Guid _quotationId = Guid.NewGuid();

    private static readonly DateTime CreatedAt = new(2026, 7, 10, 9, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime ObtainedAt = new(2026, 7, 11, 14, 31, 0, DateTimeKind.Utc);

    public GetQuotationDetailUseCaseTests()
        => _useCase = new GetQuotationDetailUseCase(_quotationRepository);

    private QuotationDetailDto Detail(
        EQuotationResult result = EQuotationResult.ReadyForEmission,
        bool requiresCcg = false,
        bool ccgSigned = false,
        string? number = "PROP-1",
        DateTime? obtainedAt = null,
        IReadOnlyList<QuotationDetailCoverageDto>? coverages = null)
        => new(
            QuotationId: _quotationId,
            Number: number,
            PolicyHolderName: "Pilão Engenharia Ltda",
            PolicyHolderDocumentNumber: "10203456000142",
            InsuredName: "Secretaria Municipal",
            InsuredDocumentNumber: "27310888000131",
            InsurerId: Guid.NewGuid(),
            InsurerName: "Newe Seguros",
            InsurerLogoUrl: "https://cdn/newe.png",
            ModalityId: Guid.NewGuid(),
            ModalityName: "Executante Fornecedor",
            InsuredAmount: 1_000_000m,
            Premium: 3_600m,
            CommissionPercentage: 25m,
            CommissionValue: 900m,
            CoverageStartDate: new DateOnly(2026, 6, 29),
            CoverageEndDate: new DateOnly(2027, 6, 29),
            CreatedAt: CreatedAt,
            ObtainedAt: obtainedAt ?? ObtainedAt,
            Result: result,
            RequiresCcg: requiresCcg,
            CcgSigned: ccgSigned,
            AdditionalCoverages: coverages ?? []);

    private void Arrange(QuotationDetailDto? detail)
        => _quotationRepository.GetDetailAsync(_quotationId, _brokerageId, Arg.Any<CancellationToken>())
            .Returns(detail);

    [Fact]
    public async Task Execute_DeveRecusar_QuandoSemCorretoraAtiva()
    {
        var act = async () => await _useCase.ExecuteAsync(
            new GetQuotationDetailRequest(_quotationId, ActiveBrokerageId: null), CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
        await _quotationRepository.DidNotReceiveWithAnyArgs().GetDetailAsync(default, default, default);
    }

    [Fact]
    public async Task Execute_DeveRetornar404_QuandoDetalheNulo_ForaDoEscopoOuInexistente()
    {
        Arrange(null);

        var act = async () => await _useCase.ExecuteAsync(
            new GetQuotationDetailRequest(_quotationId, _brokerageId), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
        // RN-081: o escopo é aplicado na consulta — o use case pergunta pela Corretora ativa.
        await _quotationRepository.Received(1).GetDetailAsync(
            _quotationId, _brokerageId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_DeveMapearCampos_ComResultadoPorNomeEstavel_EComissaoPersistida()
    {
        Arrange(Detail(number: null));

        var response = await _useCase.ExecuteAsync(
            new GetQuotationDetailRequest(_quotationId, _brokerageId), CancellationToken.None);

        response.QuotationId.Should().Be(_quotationId);
        response.Number.Should().BeNull();
        response.PolicyHolderDocumentNumber.Should().Be("10203456000142");
        response.InsuredDocumentNumber.Should().Be("27310888000131");
        response.InsurerName.Should().Be("Newe Seguros");
        response.ModalityName.Should().Be("Executante Fornecedor");
        response.Result.Should().Be("ReadyForEmission");
        // Comissão em valor é a persistida — nunca recalculada no servidor a partir do prêmio.
        response.CommissionValue.Should().Be(900m);
        response.CommissionPercentage.Should().Be(25m);
    }

    [Fact]
    public async Task Execute_Cronologia_SemCcg_TrazCriadaEObtida_MaisRecentePrimeiro()
    {
        Arrange(Detail(requiresCcg: false));

        var response = await _useCase.ExecuteAsync(
            new GetQuotationDetailRequest(_quotationId, _brokerageId), CancellationToken.None);

        response.Timeline.Select(e => e.Type).Should().ContainInOrder(
            QuotationTimelineEventTypes.Obtained, QuotationTimelineEventTypes.Created);
        response.Timeline.Should().HaveCount(2);
        response.Timeline.Should().NotContain(e => e.Type == QuotationTimelineEventTypes.CcgRequired);
        response.Timeline[0].OccurredAt.Should().Be(ObtainedAt);
    }

    [Fact]
    public async Task Execute_Cronologia_ComCcg_IncluiCcgRequired_AncoradoNaObtencao()
    {
        Arrange(Detail(requiresCcg: true));

        var response = await _useCase.ExecuteAsync(
            new GetQuotationDetailRequest(_quotationId, _brokerageId), CancellationToken.None);

        response.Timeline.Select(e => e.Type).Should().ContainInOrder(
            QuotationTimelineEventTypes.CcgRequired,
            QuotationTimelineEventTypes.Obtained,
            QuotationTimelineEventTypes.Created);
        var ccg = response.Timeline.Single(e => e.Type == QuotationTimelineEventTypes.CcgRequired);
        ccg.OccurredAt.Should().Be(ObtainedAt);
    }

    [Fact]
    public async Task Execute_DeveMapearCoberturas_PorNomeEstavel()
    {
        Arrange(Detail(coverages:
        [
            new QuotationDetailCoverageDto("Trabalhista", EQuotationAdditionalCoverageStatus.Sent, "Trabalhista"),
            new QuotationDetailCoverageDto("Previdenciária", EQuotationAdditionalCoverageStatus.NotOffered, null),
        ]));

        var response = await _useCase.ExecuteAsync(
            new GetQuotationDetailRequest(_quotationId, _brokerageId), CancellationToken.None);

        response.AdditionalCoverages.Should().BeEquivalentTo(new[]
        {
            new QuotationDetailCoverageResponse("Trabalhista", "Sent", "Trabalhista"),
            new QuotationDetailCoverageResponse("Previdenciária", "NotOffered", null),
        });
    }
}

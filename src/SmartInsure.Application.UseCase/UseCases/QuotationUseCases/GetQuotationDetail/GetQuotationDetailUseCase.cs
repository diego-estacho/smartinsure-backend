using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.GetQuotationDetail.Interfaces;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.GetQuotationDetail.Requests;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.GetQuotationDetail.Responses;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Repositories.Dtos;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.UseCases.QuotationUseCases.GetQuotationDetail;

/// <summary>
/// RN-081 — detalhe read-only de uma Cotação da Corretora do Escopo ativo (RN-064). Lê o estado já
/// persistido e classificado (RN-058), sem cotar nem alterar nada. O escopo, a inclusão (mesma do livro,
/// RN-077) e a projeção vivem no repositório; aqui resolvemos o Escopo (fail-closed), traduzimos o
/// resultado e a situação das Coberturas por **nome estável** (ADR-031) e compomos a cronologia mínima.
/// </summary>
public sealed class GetQuotationDetailUseCase(IQuotationRepository quotationRepository)
    : IGetQuotationDetailUseCase
{
    public async Task<QuotationDetailResponse> ExecuteAsync(
        GetQuotationDetailRequest request, CancellationToken cancellationToken)
    {
        // RN-064/SECURITY.md: sem Corretora ativa não há detalhe a consultar (fail-closed).
        var brokerageId = request.ActiveBrokerageId
            ?? throw new ForbiddenException("Selecione a corretora ativa para consultar a cotação.");

        // RN-081: inexistência, fora da inclusão ou de outra Corretora → mesmo 404 (não revela existência).
        var detail = await quotationRepository.GetDetailAsync(
                request.QuotationId, brokerageId, cancellationToken)
            ?? throw new NotFoundException("Cotação não encontrada.");

        var coverages = detail.AdditionalCoverages
            .Select(coverage => new QuotationDetailCoverageResponse(
                coverage.Name, coverage.Status.ToString(), coverage.SentName))
            .ToList();

        return new QuotationDetailResponse(
            detail.QuotationId,
            detail.Number,
            detail.PolicyHolderName,
            detail.PolicyHolderDocumentNumber,
            detail.InsuredName,
            detail.InsuredDocumentNumber,
            detail.InsurerId,
            detail.InsurerName,
            detail.InsurerLogoUrl,
            detail.ModalityId,
            detail.ModalityName,
            detail.InsuredAmount,
            detail.Premium,
            detail.CommissionPercentage,
            detail.CommissionValue,
            detail.CoverageStartDate,
            detail.CoverageEndDate,
            detail.CreatedAt,
            detail.Result.ToString(),
            detail.RequiresCcg,
            detail.CcgSigned,
            coverages,
            BuildTimeline(detail));
    }

    /// <summary>
    /// RN-081: cronologia mínima honesta — só os marcos que a plataforma conhece do pedido (criação,
    /// obtenção do resultado e, quando a Seguradora exige, CCG), mais recente primeiro. O provedor não
    /// expõe log de eventos; nada aqui é inventado.
    /// </summary>
    private static IReadOnlyList<QuotationTimelineEventResponse> BuildTimeline(QuotationDetailDto detail)
    {
        var events = new List<QuotationTimelineEventResponse>
        {
            new(QuotationTimelineEventTypes.Created, detail.CreatedAt),
        };

        if (detail.ObtainedAt is { } obtainedAt)
        {
            events.Add(new(QuotationTimelineEventTypes.Obtained, obtainedAt));

            // CCG é veredito que chega junto da obtenção (ADR-064) — ancorado no mesmo instante.
            if (detail.RequiresCcg)
            {
                events.Add(new(QuotationTimelineEventTypes.CcgRequired, obtainedAt));
            }
        }

        // Mais recente primeiro (RN-081): montado em ordem cronológica, invertido para exibição.
        events.Reverse();
        return events;
    }
}

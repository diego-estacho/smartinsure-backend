using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.ListQuotations.Interfaces;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.ListQuotations.Requests;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.ListQuotations.Responses;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.UseCases.QuotationUseCases.ListQuotations;

/// <summary>
/// RN-057/RN-058 — Leitura do leque de Cotações do Grupo (acompanhamento por polling, ADR-051): lê o
/// estado persistido de cada Cotação por Seguradora (classificação, esteira, motivos, prêmio, CCG) e o
/// nome da Seguradora, mais a Cotação escolhida do Grupo (RN-059). Leitura barata do estado — não cota.
/// </summary>
public sealed class ListQuotationsUseCase(
    IQuotationGroupRepository quotationGroupRepository,
    IQuotationRepository quotationRepository,
    IInsurerRepository insurerRepository) : IListQuotationsUseCase
{
    public async Task<ListQuotationsResponse> ExecuteAsync(
        ListQuotationsRequest request, CancellationToken cancellationToken)
    {
        var group = await quotationGroupRepository.GetByIdAsync(request.QuotationGroupId, cancellationToken)
            ?? throw new NotFoundException("Grupo de Cotação não encontrado.");

        var quotations = await quotationRepository.ListByGroupAsync(group.Id, cancellationToken);

        var insurerIds = quotations.Select(quotation => quotation.InsurerId).Distinct().ToList();
        var insurerNames = await insurerRepository.GetCorporateNamesByIdsAsync(insurerIds, cancellationToken);
        var insurerLogos = await insurerRepository.GetLogoUrlsByIdsAsync(insurerIds, cancellationToken);

        var items = quotations
            .Select(quotation => new QuotationListItemResponse(
                quotation.Id,
                quotation.InsurerId,
                insurerNames.TryGetValue(quotation.InsurerId, out var name) ? name : "Seguradora",
                insurerLogos.TryGetValue(quotation.InsurerId, out var logo) ? logo : null,
                quotation.ProcessingStatus.ToString(),
                quotation.Result?.ToString(),
                quotation.AnalysisTrack?.ToString(),
                quotation.IsFollowable,
                quotation.Premium,
                quotation.CommissionPercentage,
                quotation.CommissionValue,
                quotation.Tax,
                quotation.AvailableLimit,
                quotation.RequiresCcg,
                quotation.CcgMaxLimitWithoutNeed,
                quotation.CcgSigned,
                quotation.Reasons.Select(reason => reason.Text).ToList()))
            .ToList();

        return new ListQuotationsResponse(group.Id, group.SelectedQuotationId, items);
    }
}

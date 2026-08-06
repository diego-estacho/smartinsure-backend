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
    IInsurerRepository insurerRepository,
    IAdditionalCoverageRepository additionalCoverageRepository) : IListQuotationsUseCase
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

        // RN-106: a tela apresenta a cobertura pelo nome CANÔNICO — o nome de origem enviado à
        // Seguradora fica em SentName, para rastreio.
        var coverageIds = quotations
            .SelectMany(quotation => quotation.AdditionalCoverages)
            .Select(coverage => coverage.AdditionalCoverageId)
            .Distinct()
            .ToList();

        var coverageNames = await additionalCoverageRepository.GetNamesByIdsAsync(
            coverageIds, cancellationToken);

        var items = quotations
            .Select(quotation => new QuotationListItemResponse(
                quotation.Id,
                quotation.ProposalNumber,
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
                quotation.Reasons.Select(reason => reason.Text).ToList(),
                quotation.AdditionalCoverages
                    .Select(coverage => new QuotationAdditionalCoverageResponse(
                        coverage.AdditionalCoverageId,
                        coverageNames.TryGetValue(coverage.AdditionalCoverageId, out var coverageName)
                            ? coverageName
                            : "Cobertura adicional",
                        coverage.Status.ToString(),
                        coverage.SentName))
                    .ToList(),
                // RN-505/RN-510: pagamento e documentos vão na leitura — é daqui que a etapa de emissão
                // monta as escolhas, sem acionar o provedor outra vez.
                quotation.ReadInstallmentOptions()
                    .Select(option => new QuotationInstallmentOptionResponse(
                        option.Number, option.Description, option.Value, option.HasInterest))
                    .ToList(),
                quotation.ReadPossibleGracePeriodsInDays(),
                quotation.ReadRequiredDocuments()
                    .Select(document => new QuotationRequiredDocumentResponse(document.Name, document.Description))
                    .ToList()))
            .ToList();

        return new ListQuotationsResponse(group.Id, group.SelectedQuotationId, items);
    }
}

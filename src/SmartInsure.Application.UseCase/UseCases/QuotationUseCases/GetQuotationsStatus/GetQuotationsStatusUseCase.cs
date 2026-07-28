using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.GetQuotationsStatus.Interfaces;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.GetQuotationsStatus.Requests;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.GetQuotationsStatus.Responses;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.UseCases.QuotationUseCases.GetQuotationsStatus;

/// <summary>RN-057/ADR-051: estado atual do fan-out de um Grupo, por leitura do estado persistido.</summary>
public sealed class GetQuotationsStatusUseCase(
    IQuotationGroupRepository groupRepository,
    IQuotationRepository quotationRepository) : IGetQuotationsStatusUseCase
{
    public async Task<QuotationsStatusResponse> ExecuteAsync(
        GetQuotationsStatusRequest request, CancellationToken cancellationToken)
    {
        var group = await groupRepository.GetByIdWithInsurersAsync(request.QuotationGroupId, cancellationToken)
            ?? throw new NotFoundException("Grupo de Cotação não encontrado.");

        var quotations = await quotationRepository.ListByGroupAsync(request.QuotationGroupId, cancellationToken);

        var items = quotations
            .Select(quotation => new QuotationItemResponse(
                quotation.Id,
                quotation.InsurerId,
                quotation.ProcessingStatus.ToString(),
                quotation.Result?.ToString(),
                quotation.AnalysisTrack?.ToString(),
                quotation.Premium,
                quotation.CommissionPercentage,
                quotation.CommissionValue,
                quotation.Tax,
                quotation.AvailableLimit,
                quotation.RequiresCcg,
                quotation.CcgMaxLimitWithoutNeed,
                quotation.CcgSigned,
                quotation.IsFollowable,
                quotation.ObtainedAt,
                quotation.Reasons.Select(reason => reason.Text).ToList()))
            .ToList();

        var pending = items.Count(item => item.ProcessingStatus == nameof(EQuotationProcessingStatus.Requested));
        var obtained = items.Count(item => item.ProcessingStatus == nameof(EQuotationProcessingStatus.Obtained));
        var failed = items.Count(item => item.ProcessingStatus == nameof(EQuotationProcessingStatus.Failed));

        return new QuotationsStatusResponse(
            group.Id,
            group.SelectedQuotationId,
            items.Count,
            obtained,
            failed,
            pending,
            items.Count > 0 && pending == 0,
            items);
    }
}

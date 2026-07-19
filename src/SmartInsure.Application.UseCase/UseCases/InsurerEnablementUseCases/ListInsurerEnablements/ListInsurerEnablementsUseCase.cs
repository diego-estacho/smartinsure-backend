using SmartInsure.Application.UseCase.ModelsBase;
using SmartInsure.Application.UseCase.UseCases.InsurerEnablementUseCases.ListInsurerEnablements.Interfaces;
using SmartInsure.Application.UseCase.UseCases.InsurerEnablementUseCases.ListInsurerEnablements.Requests;
using SmartInsure.Application.UseCase.UseCases.InsurerEnablementUseCases.ListInsurerEnablements.Responses;
using SmartInsure.Core.Abstractions.Repositories;

namespace SmartInsure.Application.UseCase.UseCases.InsurerEnablementUseCases.ListInsurerEnablements;

/// <summary>RN-022 — consulta das Habilitações de Seguradora (Inativas permanecem consultáveis).</summary>
public sealed class ListInsurerEnablementsUseCase(
    IInsurerEnablementRepository enablementRepository) : IListInsurerEnablementsUseCase
{
    public async Task<PagedResponse<InsurerEnablementListItemResponse>> ExecuteAsync(
        ListInsurerEnablementsRequest request,
        CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize is < 1 or > 100 ? 20 : request.PageSize;

        var (items, totalCount) = await enablementRepository.ListAsync(
            request.BrokerageId, page, pageSize, cancellationToken);

        var responses = items
            .Select(item => new InsurerEnablementListItemResponse(
                item.Id,
                item.BrokerageId,
                item.BrokerageName,
                item.InsurerId,
                item.InsurerCorporateName,
                item.CalculationEngine,
                item.Status))
            .ToList();

        return new PagedResponse<InsurerEnablementListItemResponse>(responses, page, pageSize, totalCount);
    }
}

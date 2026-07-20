using SmartInsure.Application.UseCase.ModelsBase;
using SmartInsure.Application.UseCase.UseCases.BrokerageInsurerEnablementUseCases.ListBrokerageInsurerEnablements.Interfaces;
using SmartInsure.Application.UseCase.UseCases.BrokerageInsurerEnablementUseCases.ListBrokerageInsurerEnablements.Requests;
using SmartInsure.Application.UseCase.UseCases.BrokerageInsurerEnablementUseCases.ListBrokerageInsurerEnablements.Responses;
using SmartInsure.Core.Abstractions.Repositories;

namespace SmartInsure.Application.UseCase.UseCases.BrokerageInsurerEnablementUseCases.ListBrokerageInsurerEnablements;

/// <summary>RN-022 — consulta das Habilitações de Seguradora (Inativas permanecem consultáveis).</summary>
public sealed class ListBrokerageInsurerEnablementsUseCase(
    IBrokerageInsurerEnablementRepository enablementRepository) : IListBrokerageInsurerEnablementsUseCase
{
    public async Task<PagedResponse<BrokerageInsurerEnablementListItemResponse>> ExecuteAsync(
        ListBrokerageInsurerEnablementsRequest request,
        CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize is < 1 or > 100 ? 20 : request.PageSize;

        var (items, totalCount) = await enablementRepository.ListAsync(
            request.BrokerageId, page, pageSize, cancellationToken);

        var responses = items
            .Select(item => new BrokerageInsurerEnablementListItemResponse(
                item.Id,
                item.BrokerageId,
                item.BrokerageName,
                item.InsurerId,
                item.InsurerCorporateName,
                item.CalculationEngine,
                item.Status))
            .ToList();

        return new PagedResponse<BrokerageInsurerEnablementListItemResponse>(responses, page, pageSize, totalCount);
    }
}

using SmartInsure.Application.UseCase.ModelsBase;
using SmartInsure.Application.UseCase.UseCases.ModalityUseCases.ListModalities.Interfaces;
using SmartInsure.Application.UseCase.UseCases.ModalityUseCases.ListModalities.Requests;
using SmartInsure.Application.UseCase.UseCases.ModalityUseCases.ListModalities.Responses;
using SmartInsure.Core.Abstractions.Repositories;

namespace SmartInsure.Application.UseCase.UseCases.ModalityUseCases.ListModalities;

/// <summary>RN-033/RN-036 — consulta padrão retorna somente Ativas; visão completa só para o Administrador.</summary>
public sealed class ListModalitiesUseCase(IModalityRepository modalityRepository) : IListModalitiesUseCase
{
    public async Task<PagedResponse<ModalityListItemResponse>> ExecuteAsync(
        ListModalitiesRequest request,
        CancellationToken cancellationToken)
    {
        var page = Math.Max(request.Page, 1);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var includeInactive = request.IncludeInactive && request.CallerIsSystemAdministrator;

        var (items, totalCount) = await modalityRepository.ListAsync(
            page, pageSize, includeInactive, cancellationToken);

        var responses = items
            .Select(item => new ModalityListItemResponse(
                item.Id, item.Name, item.ModalityGroupId, item.ModalityGroupName, item.Description, item.Status))
            .ToList();

        return new PagedResponse<ModalityListItemResponse>(responses, page, pageSize, totalCount);
    }
}

using SmartInsure.Application.UseCase.ModelsBase;
using SmartInsure.Application.UseCase.UseCases.ModalityGroupUseCases.ListModalityGroups.Interfaces;
using SmartInsure.Application.UseCase.UseCases.ModalityGroupUseCases.ListModalityGroups.Requests;
using SmartInsure.Application.UseCase.UseCases.ModalityGroupUseCases.ListModalityGroups.Responses;
using SmartInsure.Core.Abstractions.Repositories;

namespace SmartInsure.Application.UseCase.UseCases.ModalityGroupUseCases.ListModalityGroups;

/// <summary>RN-036 — consulta padrão retorna somente Ativos; visão completa só para o Administrador do Sistema.</summary>
public sealed class ListModalityGroupsUseCase(IModalityGroupRepository modalityGroupRepository)
    : IListModalityGroupsUseCase
{
    public async Task<PagedResponse<ModalityGroupListItemResponse>> ExecuteAsync(
        ListModalityGroupsRequest request,
        CancellationToken cancellationToken)
    {
        var page = Math.Max(request.Page, 1);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var includeInactive = request.IncludeInactive && request.CallerIsSystemAdministrator;

        var (items, totalCount) = await modalityGroupRepository.ListAsync(
            page, pageSize, includeInactive, cancellationToken);

        var responses = items
            .Select(item => new ModalityGroupListItemResponse(
                item.Id, item.Name, item.Description, item.DisplayOrder, item.Status))
            .ToList();

        return new PagedResponse<ModalityGroupListItemResponse>(responses, page, pageSize, totalCount);
    }
}

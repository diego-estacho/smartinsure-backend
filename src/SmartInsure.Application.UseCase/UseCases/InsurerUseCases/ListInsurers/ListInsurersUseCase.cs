using SmartInsure.Application.UseCase.ModelsBase;
using SmartInsure.Application.UseCase.UseCases.InsurerUseCases.ListInsurers.Interfaces;
using SmartInsure.Application.UseCase.UseCases.InsurerUseCases.ListInsurers.Requests;
using SmartInsure.Application.UseCase.UseCases.InsurerUseCases.ListInsurers.Responses;
using SmartInsure.Core.Abstractions.Repositories;

namespace SmartInsure.Application.UseCase.UseCases.InsurerUseCases.ListInsurers;

/// <summary>RN-008 — consulta padrão retorna somente Ativas.</summary>
public sealed class ListInsurersUseCase(IInsurerRepository insurerRepository) : IListInsurersUseCase
{
    public async Task<PagedResponse<InsurerListItemResponse>> ExecuteAsync(
        ListInsurersRequest request,
        CancellationToken cancellationToken)
    {
        var page = Math.Max(request.Page, 1);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var includeInactive = request.IncludeInactive && request.CallerIsSystemAdministrator;

        var (items, totalCount) = await insurerRepository.ListAsync(
            page, pageSize, includeInactive, cancellationToken);

        var responses = items
            .Select(item => new InsurerListItemResponse(
                item.Id, item.Cnpj, item.CorporateName, item.TradeName, item.LogoUrl, item.Status))
            .ToList();

        return new PagedResponse<InsurerListItemResponse>(
            responses, page, pageSize, totalCount);
    }
}

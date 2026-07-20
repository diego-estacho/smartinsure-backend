using SmartInsure.Application.UseCase.ModelsBase;
using SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.ListPolicyHolders.Interfaces;
using SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.ListPolicyHolders.Requests;
using SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.ListPolicyHolders.Responses;
using SmartInsure.Core.Abstractions.Repositories;

namespace SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.ListPolicyHolders;

/// <summary>RN-025 — Pessoas jurídicas com papel Tomador, filtradas por search opcional.</summary>
public sealed class ListPolicyHoldersUseCase(IPersonRepository personRepository) : IListPolicyHoldersUseCase
{
    public async Task<PagedResponse<PolicyHolderListItemResponse>> ExecuteAsync(
        ListPolicyHoldersRequest request,
        CancellationToken cancellationToken)
    {
        var page = Math.Max(request.Page, 1);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var (items, totalCount) = await personRepository.ListPolicyHoldersAsync(
            page, pageSize, request.Search, cancellationToken);

        var responses = items
            .Select(item => new PolicyHolderListItemResponse(
                item.Id,
                item.DocumentNumber,
                item.Name,
                item.SocialName,
                item.IsPrivateSector))
            .ToList();

        return new PagedResponse<PolicyHolderListItemResponse>(
            responses, page, pageSize, totalCount);
    }
}

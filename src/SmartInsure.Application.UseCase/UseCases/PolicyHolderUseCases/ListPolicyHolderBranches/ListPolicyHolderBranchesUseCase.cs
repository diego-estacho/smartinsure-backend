using SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.ListPolicyHolderBranches.Interfaces;
using SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.ListPolicyHolderBranches.Requests;
using SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.ListPolicyHolderBranches.Responses;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.ListPolicyHolderBranches;

/// <summary>RN-052/RN-025 — lista as Filiais cadastradas e vinculadas ao Tomador (matriz).</summary>
public sealed class ListPolicyHolderBranchesUseCase(IPersonRepository personRepository)
    : IListPolicyHolderBranchesUseCase
{
    public async Task<ListPolicyHolderBranchesResponse> ExecuteAsync(
        ListPolicyHolderBranchesRequest request,
        CancellationToken cancellationToken)
    {
        var policyHolder = await personRepository.GetByIdWithRolesAsync(
            request.PolicyHolderId, cancellationToken);

        if (policyHolder is null || policyHolder.GetRole(EPersonRole.PolicyHolder) is null)
        {
            throw new NotFoundException("Tomador não encontrado.");
        }

        var branches = await personRepository.ListBranchesAsync(
            request.PolicyHolderId, cancellationToken);

        return new ListPolicyHolderBranchesResponse(
            [.. branches.Select(branch => new PolicyHolderBranchResponse(
                branch.Id, branch.DocumentNumber, branch.Name, branch.SocialName))]);
    }
}

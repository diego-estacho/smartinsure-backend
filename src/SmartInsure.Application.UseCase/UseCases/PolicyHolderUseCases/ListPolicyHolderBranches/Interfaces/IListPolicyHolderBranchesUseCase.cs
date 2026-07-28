using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.ListPolicyHolderBranches.Requests;
using SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.ListPolicyHolderBranches.Responses;

namespace SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.ListPolicyHolderBranches.Interfaces;

public interface IListPolicyHolderBranchesUseCase
    : IUseCase<ListPolicyHolderBranchesRequest, ListPolicyHolderBranchesResponse>
{
}

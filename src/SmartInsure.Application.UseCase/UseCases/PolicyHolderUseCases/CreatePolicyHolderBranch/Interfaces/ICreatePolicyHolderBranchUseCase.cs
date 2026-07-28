using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.CreatePolicyHolderBranch.Requests;
using SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.CreatePolicyHolderBranch.Responses;

namespace SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.CreatePolicyHolderBranch.Interfaces;

public interface ICreatePolicyHolderBranchUseCase
    : IUseCase<CreatePolicyHolderBranchRequest, CreatePolicyHolderBranchResponse>
{
}

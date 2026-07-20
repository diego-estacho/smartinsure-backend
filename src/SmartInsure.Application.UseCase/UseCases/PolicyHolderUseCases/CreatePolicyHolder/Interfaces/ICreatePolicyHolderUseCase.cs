using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.CreatePolicyHolder.Requests;
using SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.CreatePolicyHolder.Responses;

namespace SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.CreatePolicyHolder.Interfaces;

public interface ICreatePolicyHolderUseCase : IUseCase<CreatePolicyHolderRequest, CreatePolicyHolderResponse>
{
}

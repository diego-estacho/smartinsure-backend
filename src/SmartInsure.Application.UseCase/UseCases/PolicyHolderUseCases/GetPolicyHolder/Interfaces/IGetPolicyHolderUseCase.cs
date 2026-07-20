using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.GetPolicyHolder.Requests;
using SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.GetPolicyHolder.Responses;

namespace SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.GetPolicyHolder.Interfaces;

public interface IGetPolicyHolderUseCase : IUseCase<GetPolicyHolderRequest, GetPolicyHolderResponse>
{
}

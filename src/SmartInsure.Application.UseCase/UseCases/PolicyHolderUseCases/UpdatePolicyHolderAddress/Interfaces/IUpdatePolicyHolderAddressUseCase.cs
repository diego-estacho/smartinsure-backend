using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.UpdatePolicyHolderAddress.Requests;

namespace SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.UpdatePolicyHolderAddress.Interfaces;

public interface IUpdatePolicyHolderAddressUseCase : IUseCase<UpdatePolicyHolderAddressRequest, Unit>
{
}

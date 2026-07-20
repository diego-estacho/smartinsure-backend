using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.AddPolicyHolderAddress.Requests;

namespace SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.AddPolicyHolderAddress.Interfaces;

public interface IAddPolicyHolderAddressUseCase : IUseCase<AddPolicyHolderAddressRequest, Unit>
{
}

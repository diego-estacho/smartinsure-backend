using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.RemovePolicyHolderAddress.Requests;

namespace SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.RemovePolicyHolderAddress.Interfaces;

public interface IRemovePolicyHolderAddressUseCase : IUseCase<RemovePolicyHolderAddressRequest, Unit>
{
}

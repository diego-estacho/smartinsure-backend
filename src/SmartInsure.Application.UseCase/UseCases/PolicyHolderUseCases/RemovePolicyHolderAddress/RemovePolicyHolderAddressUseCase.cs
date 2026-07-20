using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.RemovePolicyHolderAddress.Interfaces;
using SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.RemovePolicyHolderAddress.Requests;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.RemovePolicyHolderAddress;

/// <summary>RN-026 — remove endereço complementar do Tomador (nunca o principal).</summary>
public sealed class RemovePolicyHolderAddressUseCase(
    IPersonRepository personRepository,
    IUnitOfWork unitOfWork) : IRemovePolicyHolderAddressUseCase
{
    public async Task<Unit> ExecuteAsync(
        RemovePolicyHolderAddressRequest request,
        CancellationToken cancellationToken)
    {
        var policyHolder = await personRepository.GetTrackedPolicyHolderByIdAsync(
            request.PolicyHolderId, cancellationToken)
            ?? throw new NotFoundException("Tomador não encontrado.");

        policyHolder.RemoveAdditionalAddress(request.AddressId);

        await unitOfWork.CommitAsync(cancellationToken);

        return Unit.Value;
    }
}

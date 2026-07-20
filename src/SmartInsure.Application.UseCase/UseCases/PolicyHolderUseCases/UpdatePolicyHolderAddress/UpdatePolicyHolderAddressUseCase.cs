using SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.UpdatePolicyHolderAddress.Interfaces;
using SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.UpdatePolicyHolderAddress.Requests;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Exceptions;
using SmartInsure.Application.UseCase.Common;

namespace SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.UpdatePolicyHolderAddress;

/// <summary>RN-026 — altera endereço complementar do Tomador (nunca o principal).</summary>
public sealed class UpdatePolicyHolderAddressUseCase(
    IPersonRepository personRepository,
    IUnitOfWork unitOfWork) : IUpdatePolicyHolderAddressUseCase
{
    public async Task<Unit> ExecuteAsync(
        UpdatePolicyHolderAddressRequest request,
        CancellationToken cancellationToken)
    {
        var policyHolder = await personRepository.GetTrackedPolicyHolderByIdAsync(
            request.PolicyHolderId, cancellationToken)
            ?? throw new NotFoundException("Tomador não encontrado.");

        policyHolder.UpdateAdditionalAddress(
            request.AddressId,
            request.ZipCode,
            request.Street,
            request.Number,
            request.Complement,
            request.Neighborhood,
            request.City,
            request.State);

        await unitOfWork.CommitAsync(cancellationToken);

        return Unit.Value;
    }
}

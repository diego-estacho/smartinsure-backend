using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.AddPolicyHolderAddress.Interfaces;
using SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.AddPolicyHolderAddress.Requests;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.AddPolicyHolderAddress;

/// <summary>RN-026 — adiciona endereço complementar ao Tomador.</summary>
public sealed class AddPolicyHolderAddressUseCase(
    IPersonRepository personRepository,
    IUnitOfWork unitOfWork) : IAddPolicyHolderAddressUseCase
{
    public async Task<Unit> ExecuteAsync(
        AddPolicyHolderAddressRequest request,
        CancellationToken cancellationToken)
    {
        var policyHolder = await personRepository.GetTrackedPolicyHolderByIdAsync(
            request.PolicyHolderId, cancellationToken)
            ?? throw new NotFoundException("Tomador não encontrado.");

        policyHolder.AddAdditionalAddress(
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

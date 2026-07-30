using SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.GetPolicyHolder.Interfaces;
using SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.GetPolicyHolder.Requests;
using SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.GetPolicyHolder.Responses;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.GetPolicyHolder;

/// <summary>RN-025 — detalhes do Tomador a partir da Pessoa jurídica com papel PolicyHolder.</summary>
public sealed class GetPolicyHolderUseCase(IPersonRepository personRepository) : IGetPolicyHolderUseCase
{
    public async Task<GetPolicyHolderResponse> ExecuteAsync(
        GetPolicyHolderRequest request,
        CancellationToken cancellationToken)
    {
        var policyHolder = await personRepository.GetPolicyHolderByIdAsync(
            request.PolicyHolderId, cancellationToken)
            ?? throw new NotFoundException("Tomador não encontrado.");

        // RN-101: as Filiais cadastradas e vinculadas ao Tomador aparecem na sua ficha.
        var branches = await personRepository.ListBranchesAsync(policyHolder.Id, cancellationToken);

        return new GetPolicyHolderResponse(
            policyHolder.Id,
            policyHolder.DocumentNumber,
            policyHolder.Name,
            policyHolder.SocialName,
            policyHolder.LegalNatureCode,
            policyHolder.LegalNatureDescription,
            policyHolder.IsPrivateSector,
            policyHolder.Addresses
                .Select(address => new PolicyHolderAddressResponse(
                    address.Id,
                    address.ZipCode,
                    address.Street,
                    address.Number,
                    address.Complement,
                    address.Neighborhood,
                    address.City,
                    address.State,
                    address.IsMain))
                .ToList(),
            policyHolder.Appointments
                .Select(appointment => new PolicyHolderAppointmentResponse(
                    appointment.Id,
                    appointment.InsurerId,
                    appointment.InsurerDocumentNumber,
                    appointment.InsurerName,
                    appointment.BrokerageId,
                    appointment.BrokerageDocumentNumber,
                    appointment.BrokerageName,
                    appointment.Status,
                    appointment.StartedAt,
                    appointment.EndedAt))
                .ToList(),
            branches
                .Select(branch => new PolicyHolderBranchResponse(
                    branch.Id,
                    branch.DocumentNumber,
                    branch.Name,
                    branch.SocialName))
                .ToList());
    }
}

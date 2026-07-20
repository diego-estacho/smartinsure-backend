using SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.CreatePolicyHolderAppointment.Interfaces;
using SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.CreatePolicyHolderAppointment.Requests;
using SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.CreatePolicyHolderAppointment.Responses;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.CreatePolicyHolderAppointment;

/// <summary>
/// RN-027/RN-028 — cria Nomeação de Tomador validando Corretora e Seguradora Ativas.
/// Se há vigente com a mesma Corretora: ConflictException.
/// Se há vigente com Corretora diferente: substitui (End + Create na mesma transação).
/// </summary>
public sealed class CreatePolicyHolderAppointmentUseCase(
    IPersonRepository personRepository,
    IInsurerRepository insurerRepository,
    IPolicyHolderAppointmentRepository appointmentRepository,
    IUnitOfWork unitOfWork) : ICreatePolicyHolderAppointmentUseCase
{
    public async Task<CreatePolicyHolderAppointmentResponse> ExecuteAsync(
        CreatePolicyHolderAppointmentRequest request,
        CancellationToken cancellationToken)
    {
        // Validar que o Tomador existe com papel PolicyHolder
        var policyHolder = await personRepository.GetTrackedPolicyHolderByIdAsync(
            request.PolicyHolderId, cancellationToken)
            ?? throw new NotFoundException("Tomador não encontrado.");

        // Validar que a Corretora existe com papel Broker e Status Active
        var brokerage = await personRepository.GetTrackedBrokerageByIdAsync(
            request.BrokerageId, cancellationToken)
            ?? throw new NotFoundException("Corretora não encontrada.");

        var brokerageRole = brokerage.GetRole(EPersonRole.Broker);
        if (brokerageRole?.Status != EPersonRoleStatus.Active)
        {
            throw new BusinessRuleException("A corretora não está ativa.");
        }

        // Validar que a Seguradora existe e tem Status Active
        var insurer = await insurerRepository.GetTrackedByIdAsync(
            request.InsurerId, cancellationToken)
            ?? throw new NotFoundException("Seguradora não encontrada.");

        if (insurer.Status != EInsurerStatus.Active)
        {
            throw new BusinessRuleException("A seguradora não está ativa.");
        }

        // Verificar se já existe Nomeação Vigente para este par Tomador×Seguradora
        var activeAppointment = await appointmentRepository.GetTrackedActiveByPairAsync(
            request.PolicyHolderId, request.InsurerId, cancellationToken);

        PolicyHolderAppointment newAppointment;

        if (activeAppointment is not null)
        {
            // Se a Corretora é a mesma: conflito
            if (activeAppointment.BrokerageId == request.BrokerageId)
            {
                throw new ConflictException(
                    "A corretora informada já é a nomeada vigente para esta seguradora.");
            }

            // Se a Corretora é diferente: substituir (End + Create com transação explícita)
            // RN-028: usar duas flushes para garantir atomicidade da substituição
            // Índice único filtrado (PolicyHolderId, InsurerId) WHERE Status='Active' não é deferível;
            // sem UPDATE antes de INSERT, o unique viola se INSERT executar primeiro.
            await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                activeAppointment.End();
                await unitOfWork.CommitAsync(cancellationToken);  // Flush #1: old row -> Ended

                // Criar a nova Nomeação
                newAppointment = PolicyHolderAppointment.Create(
                    request.PolicyHolderId, request.BrokerageId, request.InsurerId);

                await appointmentRepository.AddAsync(newAppointment, cancellationToken);
                await unitOfWork.CommitAsync(cancellationToken);  // Flush #2: INSERT Active

                await transaction.CommitAsync(cancellationToken); // Commit DB transaction
            }
            catch
            {
                // EF's IDbContextTransaction.DisposeAsync rolls back if not committed
                throw;
            }
        }
        else
        {
            // Criar a nova Nomeação (sem existente)
            newAppointment = PolicyHolderAppointment.Create(
                request.PolicyHolderId, request.BrokerageId, request.InsurerId);

            await appointmentRepository.AddAsync(newAppointment, cancellationToken);
            await unitOfWork.CommitAsync(cancellationToken);
        }

        return new CreatePolicyHolderAppointmentResponse(
            newAppointment.Id,
            newAppointment.PolicyHolderId,
            newAppointment.BrokerageId,
            newAppointment.InsurerId,
            newAppointment.Status.ToString(),
            newAppointment.StartedAt,
            newAppointment.EndedAt);
    }
}

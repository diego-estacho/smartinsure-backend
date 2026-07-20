using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.EndPolicyHolderAppointment.Interfaces;
using SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.EndPolicyHolderAppointment.Requests;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.EndPolicyHolderAppointment;

/// <summary>RN-028 — encerra uma Nomeação de Tomador sem condições de status da Corretora ou Seguradora.</summary>
public sealed class EndPolicyHolderAppointmentUseCase(
    IPolicyHolderAppointmentRepository appointmentRepository,
    IUnitOfWork unitOfWork) : IEndPolicyHolderAppointmentUseCase
{
    public async Task<Unit> ExecuteAsync(
        EndPolicyHolderAppointmentRequest request,
        CancellationToken cancellationToken)
    {
        var appointment = await appointmentRepository.GetTrackedByIdAsync(
            request.AppointmentId, cancellationToken)
            ?? throw new NotFoundException("Nomeação não encontrada.");

        appointment.End();

        await unitOfWork.CommitAsync(cancellationToken);

        return Unit.Value;
    }
}

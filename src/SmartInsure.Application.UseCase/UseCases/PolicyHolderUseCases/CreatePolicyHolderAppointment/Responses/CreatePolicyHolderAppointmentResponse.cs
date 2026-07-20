namespace SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.CreatePolicyHolderAppointment.Responses;

public sealed record CreatePolicyHolderAppointmentResponse(
    Guid Id,
    Guid PolicyHolderId,
    Guid BrokerageId,
    Guid InsurerId,
    string Status,
    DateTime StartedAt,
    DateTime? EndedAt);

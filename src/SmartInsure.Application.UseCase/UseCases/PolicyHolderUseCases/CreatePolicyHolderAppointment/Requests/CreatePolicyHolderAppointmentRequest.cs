namespace SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.CreatePolicyHolderAppointment.Requests;

public sealed record CreatePolicyHolderAppointmentRequest(
    Guid PolicyHolderId,
    Guid BrokerageId,
    Guid InsurerId);

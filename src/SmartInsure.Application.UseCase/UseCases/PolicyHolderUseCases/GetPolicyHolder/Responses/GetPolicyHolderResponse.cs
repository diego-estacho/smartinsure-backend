namespace SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.GetPolicyHolder.Responses;

public sealed record GetPolicyHolderResponse(
    Guid Id,
    string DocumentNumber,
    string Name,
    string? SocialName,
    string? LegalNatureCode,
    string? LegalNatureDescription,
    bool? IsPrivateSector,
    IReadOnlyList<PolicyHolderAddressResponse> Addresses,
    IReadOnlyList<PolicyHolderAppointmentResponse> Appointments);

public sealed record PolicyHolderAddressResponse(
    Guid Id,
    string? ZipCode,
    string? Street,
    string? Number,
    string? Complement,
    string? Neighborhood,
    string? City,
    string? State,
    bool IsMain);

public sealed record PolicyHolderAppointmentResponse(
    Guid Id,
    Guid InsurerId,
    string InsurerDocumentNumber,
    string InsurerName,
    Guid BrokerageId,
    string BrokerageDocumentNumber,
    string BrokerageName,
    string Status,
    DateTime StartedAt,
    DateTime? EndedAt);

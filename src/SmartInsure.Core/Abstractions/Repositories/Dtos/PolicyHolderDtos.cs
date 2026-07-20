namespace SmartInsure.Core.Abstractions.Repositories.Dtos;

public sealed record PolicyHolderListItemDto(
    Guid Id,
    string DocumentNumber,
    string Name,
    string? SocialName,
    bool? IsPrivateSector);

public sealed record PolicyHolderDetailsDto(
    Guid Id,
    string DocumentNumber,
    string Name,
    string? SocialName,
    string? LegalNatureCode,
    string? LegalNatureDescription,
    bool? IsPrivateSector,
    IReadOnlyList<PersonAddressDetailsDto> Addresses,
    IReadOnlyList<PolicyHolderAppointmentDetailDto> Appointments);

public sealed record PersonAddressDetailsDto(
    Guid Id,
    string? ZipCode,
    string? Street,
    string? Number,
    string? Complement,
    string? Neighborhood,
    string? City,
    string? State,
    bool IsMain);

public sealed record PolicyHolderAppointmentDetailDto(
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

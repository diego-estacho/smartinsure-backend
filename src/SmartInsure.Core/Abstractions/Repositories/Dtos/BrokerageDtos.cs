namespace SmartInsure.Core.Abstractions.Repositories.Dtos;

public sealed record BrokerageListItemDto(
    Guid Id,
    string DocumentNumber,
    string Name,
    string? SocialName,
    bool? IsPrivateSector,
    string Status);

public sealed record BrokerageDetailsDto(
    Guid Id,
    string DocumentNumber,
    string Name,
    string? SocialName,
    string? LegalNatureCode,
    string? LegalNatureDescription,
    bool? IsPrivateSector,
    string Status,
    PersonMainAddressDto? MainAddress);

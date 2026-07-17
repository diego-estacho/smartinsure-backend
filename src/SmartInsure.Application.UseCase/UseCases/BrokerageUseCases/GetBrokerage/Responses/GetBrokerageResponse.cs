namespace SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.GetBrokerage.Responses;

public sealed record GetBrokerageResponse(
    Guid Id,
    string DocumentNumber,
    string Name,
    string? SocialName,
    string? LegalNatureCode,
    string? LegalNatureName,
    bool? IsPrivateSector,
    string Status,
    BrokerageAddressResponse? MainAddress);

public sealed record BrokerageAddressResponse(
    string? ZipCode,
    string? Street,
    string? Number,
    string? Complement,
    string? Neighborhood,
    string? City,
    string? State);

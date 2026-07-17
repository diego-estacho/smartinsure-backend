namespace SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.CreateBrokerage.Responses;

public sealed record CreateBrokerageResponse(
    Guid Id,
    string DocumentNumber,
    string Name,
    string? SocialName,
    string? LegalNatureCode,
    string? LegalNatureName,
    bool? IsPrivateSector,
    string Status,
    ImportedBrokerageAddressResponse? MainAddress);

public sealed record ImportedBrokerageAddressResponse(
    string? ZipCode,
    string? Street,
    string? Number,
    string? Complement,
    string? Neighborhood,
    string? City,
    string? State);

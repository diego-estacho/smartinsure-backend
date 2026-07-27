using SmartInsure.Core.Abstractions.Repositories.Dtos;

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
    string Situation,
    string? ContactEmail,
    string? ContactPhone,
    string? ResponsibleName,
    DateTime RegisteredAt,
    int EnabledInsurerCount,
    ImportedBrokerageAddressResponse? MainAddress)
{
    public static CreateBrokerageResponse From(BrokerageDetailsDto brokerage)
        => new(
            brokerage.Id,
            brokerage.DocumentNumber,
            brokerage.Name,
            brokerage.SocialName,
            brokerage.LegalNatureCode,
            brokerage.LegalNatureDescription,
            brokerage.IsPrivateSector,
            brokerage.Status,
            brokerage.Situation,
            brokerage.ContactEmail,
            brokerage.ContactPhone,
            brokerage.ResponsibleName,
            brokerage.RegisteredAt,
            brokerage.EnabledInsurerCount,
            brokerage.MainAddress is null
                ? null
                : new ImportedBrokerageAddressResponse(
                    brokerage.MainAddress.ZipCode,
                    brokerage.MainAddress.Street,
                    brokerage.MainAddress.Number,
                    brokerage.MainAddress.Complement,
                    brokerage.MainAddress.Neighborhood,
                    brokerage.MainAddress.City,
                    brokerage.MainAddress.State));
}

public sealed record ImportedBrokerageAddressResponse(
    string? ZipCode,
    string? Street,
    string? Number,
    string? Complement,
    string? Neighborhood,
    string? City,
    string? State);

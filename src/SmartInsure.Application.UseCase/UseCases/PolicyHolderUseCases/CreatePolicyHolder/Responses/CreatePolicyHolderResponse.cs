namespace SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.CreatePolicyHolder.Responses;

public sealed record CreatePolicyHolderResponse(
    Guid Id,
    string DocumentNumber,
    string Name,
    string? SocialName,
    string? LegalNatureCode,
    string? LegalNatureDescription,
    bool? IsPrivateSector,
    ImportedPolicyHolderAddressResponse? MainAddress);

public sealed record ImportedPolicyHolderAddressResponse(
    string? ZipCode,
    string? Street,
    string? Number,
    string? Complement,
    string? Neighborhood,
    string? City,
    string? State);

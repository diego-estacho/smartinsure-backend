namespace SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.UpdatePolicyHolderAddress.Requests;

public sealed record UpdatePolicyHolderAddressRequest(
    Guid PolicyHolderId,
    Guid AddressId,
    string? ZipCode,
    string? Street,
    string? Number,
    string? Complement,
    string? Neighborhood,
    string? City,
    string? State);

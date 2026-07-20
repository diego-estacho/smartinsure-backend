namespace SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.AddPolicyHolderAddress.Requests;

public sealed record AddPolicyHolderAddressRequest(
    Guid PolicyHolderId,
    string? ZipCode,
    string? Street,
    string? Number,
    string? Complement,
    string? Neighborhood,
    string? City,
    string? State);

namespace SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.RemovePolicyHolderAddress.Requests;

public sealed record RemovePolicyHolderAddressRequest(Guid PolicyHolderId, Guid AddressId);

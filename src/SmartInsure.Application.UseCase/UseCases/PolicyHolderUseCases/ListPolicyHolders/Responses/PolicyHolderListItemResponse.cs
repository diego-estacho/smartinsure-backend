namespace SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.ListPolicyHolders.Responses;

public sealed record PolicyHolderListItemResponse(
    Guid Id,
    string DocumentNumber,
    string Name,
    string? SocialName,
    bool? IsPrivateSector,
    string? City,
    string? StateCode,
    bool? IsAppointedToBrokerage);

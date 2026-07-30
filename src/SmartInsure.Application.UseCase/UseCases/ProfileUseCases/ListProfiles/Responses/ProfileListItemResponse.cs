namespace SmartInsure.Application.UseCase.UseCases.ProfileUseCases.ListProfiles.Responses;

public sealed record ProfileListItemResponse(
    Guid Id,
    string Name,
    string Scope,
    bool IsFixed,
    Guid? BrokerageId,
    Guid? PolicyHolderId,
    int PermissionCount);

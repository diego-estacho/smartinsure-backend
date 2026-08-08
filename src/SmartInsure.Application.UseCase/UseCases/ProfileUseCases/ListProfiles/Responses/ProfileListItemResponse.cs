namespace SmartInsure.Application.UseCase.UseCases.ProfileUseCases.ListProfiles.Responses;

public sealed record ProfileListItemResponse(
    Guid Id,
    string Name,
    string Scope,
    bool IsFixed,
    Guid? BrokerageId,
    Guid? PolicyHolderId,
    int PermissionCount,
    string? Description,
    DateTime CreatedAt,
    int UserCount,
    int AreaCount);

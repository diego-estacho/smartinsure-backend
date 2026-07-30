namespace SmartInsure.Core.Abstractions.Repositories.Dtos;

public sealed record ProfileListItemDto(
    Guid Id,
    string Name,
    string Scope,
    bool IsFixed,
    Guid? BrokerageId,
    Guid? PolicyHolderId,
    int PermissionCount);

public sealed record ProfileDetailsDto(
    Guid Id,
    string Name,
    string Scope,
    bool IsFixed,
    Guid? BrokerageId,
    Guid? PolicyHolderId,
    IReadOnlyList<ProfilePermissionDto> Permissions);

public sealed record ProfilePermissionDto(
    Guid Id,
    string Code,
    string? Description,
    bool IsSystem);

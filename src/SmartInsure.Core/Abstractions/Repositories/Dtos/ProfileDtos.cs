namespace SmartInsure.Core.Abstractions.Repositories.Dtos;

public sealed record ProfileListItemDto(
    Guid Id,
    string Name,
    string Scope,
    bool IsFixed,
    Guid? BrokerageId,
    Guid? PolicyHolderId,
    int PermissionCount,
    string? Description = null,
    DateTime CreatedAt = default,
    int UserCount = 0,
    int AreaCount = 0);

public sealed record ProfileDetailsDto(
    Guid Id,
    string Name,
    string Scope,
    bool IsFixed,
    Guid? BrokerageId,
    Guid? PolicyHolderId,
    IReadOnlyList<ProfilePermissionDto> Permissions,
    string? Description = null,
    DateTime CreatedAt = default,
    IReadOnlyList<ProfileLinkedUserDto>? LinkedUsers = null,
    int LinkedUserCount = 0);

public sealed record ProfilePermissionDto(
    Guid Id,
    string Code,
    string? Description,
    bool IsSystem);

/// <summary>RN-074: Usuário vinculado a um Perfil (para "Quem usa" e para a migração na exclusão).</summary>
public sealed record ProfileLinkedUserDto(
    Guid Id,
    string Name,
    string Email);

/// <summary>RN-074: uso do Perfil — quantos Usuários e quantas Áreas ele toca (para a listagem).</summary>
public sealed record ProfileUsageDto(
    int UserCount,
    int AreaCount);

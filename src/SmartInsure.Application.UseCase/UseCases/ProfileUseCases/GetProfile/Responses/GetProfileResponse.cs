namespace SmartInsure.Application.UseCase.UseCases.ProfileUseCases.GetProfile.Responses;

public sealed record GetProfileResponse(
    Guid Id,
    string Name,
    string Scope,
    bool IsFixed,
    Guid? BrokerageId,
    Guid? PolicyHolderId,
    IReadOnlyList<ProfilePermissionResponse> Permissions,
    string? Description,
    DateTime CreatedAt,
    IReadOnlyList<ProfileLinkedUserResponse> LinkedUsers,
    int LinkedUserCount);

/// <summary>Permissão marcada no Perfil (RN-063): item do catálogo fixo da plataforma.</summary>
public sealed record ProfilePermissionResponse(
    Guid Id,
    string Code,
    string? Description,
    bool IsSystem);

/// <summary>RN-074: Usuário que usa este Perfil — "Quem usa este perfil" no detalhe.</summary>
public sealed record ProfileLinkedUserResponse(
    Guid Id,
    string Name,
    string Email);

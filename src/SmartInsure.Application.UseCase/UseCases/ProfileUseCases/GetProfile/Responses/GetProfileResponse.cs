namespace SmartInsure.Application.UseCase.UseCases.ProfileUseCases.GetProfile.Responses;

public sealed record GetProfileResponse(
    Guid Id,
    string Name,
    string Scope,
    bool IsFixed,
    Guid? BrokerageId,
    Guid? PolicyHolderId,
    IReadOnlyList<ProfilePermissionResponse> Permissions);

/// <summary>Permissão marcada no Perfil (RN-063): item do catálogo fixo da plataforma.</summary>
public sealed record ProfilePermissionResponse(
    Guid Id,
    string Code,
    string? Description,
    bool IsSystem);

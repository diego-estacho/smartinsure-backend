namespace SmartInsure.Application.UseCase.UseCases.ProfileUseCases.UpdateFixedProfilePermissions.Responses;

/// <summary>RN-073: Perfil fixo após a edição das Permissões (vale para todos os Escopos).</summary>
public sealed record UpdateFixedProfilePermissionsResponse(
    Guid Id,
    string Name,
    string Scope,
    int PermissionCount);

namespace SmartInsure.Application.UseCase.UseCases.ProfileUseCases.UpdateScopedProfile.Responses;

/// <summary>RN-074: Perfil customizado após a edição.</summary>
public sealed record UpdateScopedProfileResponse(
    Guid Id,
    string Name,
    string Scope,
    int PermissionCount);

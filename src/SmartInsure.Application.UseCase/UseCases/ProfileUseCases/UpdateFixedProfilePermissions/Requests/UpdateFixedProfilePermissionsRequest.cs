namespace SmartInsure.Application.UseCase.UseCases.ProfileUseCases.UpdateFixedProfilePermissions.Requests;

/// <summary>
/// RN-073: o Administrador do Sistema adiciona ou remove Permissões de um Perfil fixo. Nome e
/// Escopo do Perfil fixo são imutáveis — só o conjunto de Permissões muda, com efeito global.
/// A autorização é policy de rota (Administrador do Sistema).
/// </summary>
public sealed record UpdateFixedProfilePermissionsRequest(
    Guid ProfileId,
    IReadOnlyCollection<string> PermissionCodes);

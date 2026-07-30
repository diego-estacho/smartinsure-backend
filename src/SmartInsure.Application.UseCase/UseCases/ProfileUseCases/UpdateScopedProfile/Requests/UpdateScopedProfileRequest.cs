namespace SmartInsure.Application.UseCase.UseCases.ProfileUseCases.UpdateScopedProfile.Requests;

/// <summary>
/// RN-074: edição de Perfil customizado do próprio Escopo — nome e Permissões. O Escopo vem do
/// acesso; Perfil de outra Corretora/Tomador e Perfil fixo são recusados.
/// </summary>
public sealed record UpdateScopedProfileRequest(
    string ExternalIdentity,
    Guid? ActiveBrokerageId,
    Guid? ActivePolicyHolderId,
    Guid ProfileId,
    string Name,
    IReadOnlyCollection<string> PermissionCodes);

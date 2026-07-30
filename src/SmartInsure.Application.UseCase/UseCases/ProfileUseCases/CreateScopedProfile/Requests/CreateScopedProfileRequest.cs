namespace SmartInsure.Application.UseCase.UseCases.ProfileUseCases.CreateScopedProfile.Requests;

/// <summary>
/// RN-069/RN-070: criação de Perfil customizado no Escopo que o solicitante administra. O Escopo
/// (Corretora ativa ou Tomador ativo) vem do acesso — o cliente informa apenas nome e Permissões.
/// </summary>
public sealed record CreateScopedProfileRequest(
    string ExternalIdentity,
    Guid? ActiveBrokerageId,
    Guid? ActivePolicyHolderId,
    string Name,
    IReadOnlyCollection<string> PermissionCodes);

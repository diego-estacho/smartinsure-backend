namespace SmartInsure.Application.UseCase.UseCases.ProfileUseCases.ListAssignableProfiles.Requests;

/// <summary>
/// RN-072: Perfis que o solicitante pode atribuir na criação de Usuário, dentro do Escopo ativo.
/// Identidade e Escopo ativo vêm do acesso — o cliente não escolhe o Escopo consultado.
/// </summary>
public sealed record ListAssignableProfilesRequest(
    string ExternalIdentity,
    Guid? ActiveBrokerageId,
    Guid? ActivePolicyHolderId);

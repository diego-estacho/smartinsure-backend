namespace SmartInsure.Application.UseCase.UseCases.ProfileUseCases.GetProfile.Requests;

/// <summary>
/// RN-072: detalhe do Perfil. Identidade e Escopo ativo vêm do acesso — o Administrador do Sistema
/// vê qualquer Perfil; o administrador de Escopo, apenas os do próprio Escopo (e nunca os fixos de
/// administração, mesmo consultando pelo identificador).
/// </summary>
public sealed record GetProfileRequest(
    Guid ProfileId,
    string ExternalIdentity,
    Guid? ActiveBrokerageId,
    Guid? ActivePolicyHolderId);

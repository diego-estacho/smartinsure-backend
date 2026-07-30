namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.GetCurrentUserContext.Requests;

/// <summary>
/// RN-064: contexto do Usuário autenticado. A identidade e o Escopo ativo vêm do próprio
/// acesso (claims), nunca do corpo — o cliente não escolhe de quem é o contexto.
/// </summary>
/// <param name="ExternalIdentity">Identidade do provedor, lida do acesso.</param>
/// <param name="ActiveBrokerageId">Corretora ativa do acesso corrente, quando houver.</param>
/// <param name="ActivePolicyHolderId">Tomador ativo do acesso corrente, quando houver.</param>
public sealed record GetCurrentUserContextRequest(
    string ExternalIdentity,
    Guid? ActiveBrokerageId,
    Guid? ActivePolicyHolderId);

namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.InvitePolicyHolderAdministrator.Requests;

/// <summary>
/// RN-068: o Corretor Administrador cria um Tomador Administrador para um Tomador nomeado à
/// Corretora ativa. Identidade do solicitante e Corretora ativa vêm do acesso, nunca do corpo.
/// </summary>
/// <param name="ExternalIdentity">Identidade do solicitante, lida do acesso.</param>
/// <param name="ActiveBrokerageId">Corretora ativa do solicitante, lida do acesso.</param>
/// <param name="Name">Nome do convidado.</param>
/// <param name="Email">E-mail do convidado.</param>
/// <param name="PolicyHolderId">Tomador que o convidado vai administrar.</param>
public sealed record InvitePolicyHolderAdministratorRequest(
    string ExternalIdentity,
    Guid? ActiveBrokerageId,
    string Name,
    string Email,
    Guid PolicyHolderId);

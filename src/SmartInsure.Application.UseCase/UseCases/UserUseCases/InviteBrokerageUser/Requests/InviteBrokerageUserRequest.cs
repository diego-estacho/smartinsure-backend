namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.InviteBrokerageUser.Requests;

/// <summary>
/// RN-069: o Corretor Administrador cria um Usuário na Corretora ativa, com um Perfil do Escopo
/// daquela Corretora. Identidade do solicitante e Corretora ativa vêm do acesso, nunca do corpo.
/// </summary>
/// <param name="ExternalIdentity">Identidade do solicitante, lida do acesso.</param>
/// <param name="ActiveBrokerageId">Corretora ativa do solicitante, lida do acesso.</param>
/// <param name="Name">Nome do convidado.</param>
/// <param name="Email">E-mail do convidado.</param>
/// <param name="DocumentNumber">CPF do convidado (RN-082).</param>
/// <param name="ProfileId">Perfil a conceder, dentre os oferecidos para a Corretora ativa (RN-072).</param>
public sealed record InviteBrokerageUserRequest(
    string ExternalIdentity,
    Guid? ActiveBrokerageId,
    string Name,
    string Email,
    string DocumentNumber,
    Guid ProfileId);

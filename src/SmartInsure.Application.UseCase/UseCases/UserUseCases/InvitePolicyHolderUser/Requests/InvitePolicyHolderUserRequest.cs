namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.InvitePolicyHolderUser.Requests;

/// <summary>
/// RN-070: o Tomador Administrador cria um Usuário no Tomador ativo, com um Perfil do Escopo
/// daquele Tomador. Identidade e Tomador ativo vêm do acesso, nunca do corpo.
/// </summary>
public sealed record InvitePolicyHolderUserRequest(
    string ExternalIdentity,
    Guid? ActivePolicyHolderId,
    string Name,
    string Email,
    string DocumentNumber,
    Guid ProfileId);

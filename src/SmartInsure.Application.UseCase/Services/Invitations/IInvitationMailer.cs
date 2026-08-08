namespace SmartInsure.Application.UseCase.Services.Invitations;

/// <summary>
/// RN-065 — compõe e envia o e-mail de Convite (link de primeiro acesso). Compartilhado pelos
/// fluxos que emitem Convite (criação, convite de Corretor Administrador, reenvio).
/// </summary>
public interface IInvitationMailer
{
    Task SendAsync(string email, string userName, string plainToken, string subject, CancellationToken cancellationToken);

    /// <summary>
    /// RN-203 — envia o e-mail de redefinição de senha (Usuário Ativo). Reusa o mesmo token e a
    /// página de definição de senha do Convite (RN-065), com texto próprio de redefinição.
    /// </summary>
    Task SendPasswordResetAsync(
        string email, string userName, string plainToken, CancellationToken cancellationToken);
}

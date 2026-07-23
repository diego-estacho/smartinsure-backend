namespace SmartInsure.Application.UseCase.Services.Invitations;

/// <summary>
/// RN-035 — compõe e envia o e-mail de Convite (link de primeiro acesso). Compartilhado pelos
/// fluxos que emitem Convite (criação, convite de Corretor Administrador, reenvio).
/// </summary>
public interface IInvitationMailer
{
    Task SendAsync(string email, string userName, string plainToken, string subject, CancellationToken cancellationToken);
}

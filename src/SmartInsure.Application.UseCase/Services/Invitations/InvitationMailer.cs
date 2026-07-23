using System.Web;
using Microsoft.Extensions.Options;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Abstractions.Services.Dtos;
using SmartInsure.Infra.CrossCutting.Options;

namespace SmartInsure.Application.UseCase.Services.Invitations;

/// <summary>RN-035: monta o link a partir da URL configurada e envia o e-mail de Convite.</summary>
public sealed class InvitationMailer(
    IMailService mailService,
    IOptions<InvitationOptions> invitationOptions) : IInvitationMailer
{
    public async Task SendAsync(
        string email, string userName, string plainToken, string subject, CancellationToken cancellationToken)
    {
        var link = $"{invitationOptions.Value.AppBaseUrl}/invite?token={Uri.EscapeDataString(plainToken)}";

        await mailService.SendAsync(
            new MailMessage
            {
                To = [email],
                Subject = subject,
                HtmlBody = BuildHtml(userName, link, invitationOptions.Value.LinkExpiryDays),
            },
            cancellationToken);
    }

    private static string BuildHtml(string userName, string invitationLink, int expiryDays)
        => $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
        <h1>SmartInsure</h1>
        <p>Olá {HttpUtility.HtmlEncode(userName)},</p>
        <p>Clique no link abaixo para completar seu acesso e definir sua senha:</p>
        <p style='text-align: center; margin: 30px 0;'>
            <a href='{HttpUtility.HtmlAttributeEncode(invitationLink)}'
               style='display: inline-block; background-color: #0066cc; color: white; padding: 12px 30px; text-decoration: none; border-radius: 4px;'>
                Completar acesso
            </a>
        </p>
        <p style='color: #666; font-size: 12px;'>
            Este link expira em {expiryDays} dias. Se você não esperava este e-mail, ignore-o.
        </p>
    </div>
</body>
</html>";
}

using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Abstractions.Services.Dtos;
using SmartInsure.MailServices.Options;

namespace SmartInsure.MailServices.Services;

/// <summary>
/// Transporte SMTP via MailKit (ADR-048). Remetente fixo da configuração; falha de
/// envio é logada com contexto e propagada — o chamador decide se é crítica.
/// </summary>
public sealed class MailKitMailService(
    IOptions<MailOptions> options,
    ILogger<MailKitMailService> logger) : IMailService
{
    public async Task SendAsync(MailMessage message, CancellationToken cancellationToken)
    {
        if (message.To.Count == 0)
        {
            throw new ArgumentException(
                "A mensagem precisa de ao menos um destinatário.", nameof(message));
        }

        var settings = options.Value;
        var mime = MimeMessageFactory.Create(settings, message);

        try
        {
            using var client = new SmtpClient();

            await client.ConnectAsync(
                settings.Host,
                settings.Port,
                settings.UseSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTlsWhenAvailable,
                cancellationToken);

            await client.AuthenticateAsync(settings.User, settings.Password, cancellationToken);
            await client.SendAsync(mime, cancellationToken);
            await client.DisconnectAsync(quit: true, cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Falha ao enviar e-mail. Assunto: {Subject}; destinatários: {RecipientCount}; anexos: {AttachmentCount}.",
                message.Subject,
                message.To.Count + message.Cc.Count + message.Bcc.Count,
                message.Attachments.Count);

            throw;
        }
    }
}

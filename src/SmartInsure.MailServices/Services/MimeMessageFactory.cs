using System;
using MimeKit;
using SmartInsure.Core.Abstractions.Services.Dtos;
using SmartInsure.MailServices.Options;

namespace SmartInsure.MailServices.Services;

/// <summary>
/// Montagem do MimeMessage separada do transporte para ser testável sem servidor SMTP.
/// </summary>
internal static class MimeMessageFactory
{
    public static MimeMessage Create(MailOptions options, MailMessage message)
    {
        var mime = new MimeMessage();

        mime.From.Add(new MailboxAddress(options.DisplayName, options.From));
        AddAddresses(mime.To, message.To);
        AddAddresses(mime.Cc, message.Cc);
        AddAddresses(mime.Bcc, message.Bcc);

        if (!string.IsNullOrWhiteSpace(message.ReplyTo))
        {
            try
            {
                mime.ReplyTo.Add(MailboxAddress.Parse(message.ReplyTo));
            }
            catch (ParseException)
            {
                throw new ArgumentException($"Endereço de e-mail inválido: {message.ReplyTo}", nameof(message));
            }
        }

        mime.Subject = message.Subject;

        var body = new BodyBuilder { HtmlBody = message.HtmlBody };

        foreach (var attachment in message.Attachments)
        {
            body.Attachments.Add(
                attachment.FileName,
                attachment.Content,
                MimeKit.ContentType.Parse(attachment.ContentType));
        }

        mime.Body = body.ToMessageBody();

        return mime;
    }

    private static void AddAddresses(InternetAddressList list, IReadOnlyList<string> addresses)
    {
        foreach (var address in addresses)
        {
            try
            {
                list.Add(MailboxAddress.Parse(address));
            }
            catch (ParseException)
            {
                throw new ArgumentException($"Endereço de e-mail inválido: {address}", nameof(addresses));
            }
        }
    }
}

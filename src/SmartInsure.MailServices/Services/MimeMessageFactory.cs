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
            mime.ReplyTo.Add(MailboxAddress.Parse(message.ReplyTo));
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
            list.Add(MailboxAddress.Parse(address));
        }
    }
}

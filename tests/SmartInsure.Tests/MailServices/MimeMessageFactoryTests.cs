using FluentAssertions;
using MimeKit;
using SmartInsure.Core.Abstractions.Services.Dtos;
using SmartInsure.MailServices.Options;
using SmartInsure.MailServices.Services;

namespace SmartInsure.Tests.MailServices;

/// <summary>
/// ADR-048: remetente fixo da configuração, corpo HTML pronto, anexos em memória.
/// </summary>
public class MimeMessageFactoryTests
{
    private static readonly MailOptions Options = new()
    {
        Host = "smtp.test",
        Port = 587,
        User = "user",
        Password = "pass",
        DisplayName = "InsurePoint",
        From = "no-reply@insurepoint.com.br",
        UseSsl = false,
    };

    private static MailMessage BuildMessage() => new()
    {
        To = ["destino@teste.com.br"],
        Subject = "Assunto",
        HtmlBody = "<p>Olá</p>",
    };

    [Fact]
    public void Create_DeveUsarRemetenteDaConfiguracao_QuandoMensagemMontada()
    {
        var mime = MimeMessageFactory.Create(Options, BuildMessage());

        var from = mime.From.Mailboxes.Single();
        from.Address.Should().Be("no-reply@insurepoint.com.br");
        from.Name.Should().Be("InsurePoint");
    }

    [Fact]
    public void Create_DevePreencherTodosDestinatarios_QuandoCcEBccInformados()
    {
        var message = BuildMessage() with
        {
            To = ["a@teste.com.br", "b@teste.com.br"],
            Cc = ["c@teste.com.br"],
            Bcc = ["d@teste.com.br"],
        };

        var mime = MimeMessageFactory.Create(Options, message);

        mime.To.Mailboxes.Select(m => m.Address)
            .Should().BeEquivalentTo("a@teste.com.br", "b@teste.com.br");
        mime.Cc.Mailboxes.Single().Address.Should().Be("c@teste.com.br");
        mime.Bcc.Mailboxes.Single().Address.Should().Be("d@teste.com.br");
    }

    [Fact]
    public void Create_DeveDefinirReplyTo_QuandoInformado()
    {
        var message = BuildMessage() with { ReplyTo = "suporte@insurepoint.com.br" };

        var mime = MimeMessageFactory.Create(Options, message);

        mime.ReplyTo.Mailboxes.Single().Address.Should().Be("suporte@insurepoint.com.br");
    }

    [Fact]
    public void Create_DeveManterReplyToVazio_QuandoNaoInformado()
    {
        var mime = MimeMessageFactory.Create(Options, BuildMessage());

        mime.ReplyTo.Count.Should().Be(0);
    }

    [Fact]
    public void Create_DeveUsarCorpoHtmlPronto_QuandoMensagemMontada()
    {
        var mime = MimeMessageFactory.Create(Options, BuildMessage());

        mime.Subject.Should().Be("Assunto");
        mime.HtmlBody.Should().Be("<p>Olá</p>");
    }

    [Fact]
    public void Create_DeveAnexarBytesComNomeEContentType_QuandoAnexosInformados()
    {
        var content = new byte[] { 1, 2, 3 };
        var message = BuildMessage() with
        {
            Attachments =
            [
                new MailAttachment
                {
                    FileName = "apolice.pdf",
                    Content = content,
                    ContentType = "application/pdf",
                },
            ],
        };

        var mime = MimeMessageFactory.Create(Options, message);

        var attachment = mime.Attachments.OfType<MimePart>().Single();
        attachment.FileName.Should().Be("apolice.pdf");
        attachment.ContentType.MimeType.Should().Be("application/pdf");

        using var stream = new MemoryStream();
        attachment.Content!.DecodeTo(stream);
        stream.ToArray().Should().BeEquivalentTo(content);
    }
}

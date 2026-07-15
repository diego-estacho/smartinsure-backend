using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using SmartInsure.Core.Abstractions.Services.Dtos;
using SmartInsure.MailServices.Options;
using SmartInsure.MailServices.Services;

namespace SmartInsure.Tests.MailServices;

public class MailKitMailServiceTests
{
    [Fact]
    public async Task SendAsync_DeveLancarArgumentException_QuandoSemDestinatario()
    {
        var service = new MailKitMailService(
            Microsoft.Extensions.Options.Options.Create(new MailOptions()),
            NullLogger<MailKitMailService>.Instance);

        var message = new MailMessage
        {
            To = [],
            Subject = "Assunto",
            HtmlBody = "<p>corpo</p>",
        };

        var act = () => service.SendAsync(message, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>();
    }
}

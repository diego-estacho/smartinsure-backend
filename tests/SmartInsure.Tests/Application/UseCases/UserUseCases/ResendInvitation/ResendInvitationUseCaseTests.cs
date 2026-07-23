using FluentAssertions;
using NSubstitute;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.ResendInvitation;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.ResendInvitation.Requests;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Abstractions.Services.Dtos;
using SmartInsure.Core.Entities;
using SmartInsure.Infra.CrossCutting.Options;
using Microsoft.Extensions.Options;

namespace SmartInsure.Tests.Application.UseCases.UserUseCases.ResendInvitation;

/// <summary>RN-035 — reenvio do convite: invalida anterior + envia novo.</summary>
[Trait("RuleId", "RN-035")]
public class ResendInvitationUseCaseTests
{
    private readonly IInvitationRepository _invitationRepository = Substitute.For<IInvitationRepository>();
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IMailService _mailService = Substitute.For<IMailService>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ResendInvitationUseCase _useCase;

    public ResendInvitationUseCaseTests()
    {
        var options = Options.Create(new InvitationOptions
        {
            AppBaseUrl = "https://app.example.com",
            LinkExpiryDays = 7,
        });

        _useCase = new ResendInvitationUseCase(
            _invitationRepository, _userRepository, _mailService, _unitOfWork, options);
    }

    [Fact]
    public async Task Execute_DeveInvalidarAnteriorESenharNovo()
    {
        var user = User.Create("João", "joao@example.com", "casdoor-123");
        var oldInvitation = Invitation.Create(user.Id, 7).invitation;

        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(user);
        _invitationRepository.GetPendingByUserAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(oldInvitation);
        _mailService.SendAsync(Arg.Any<MailMessage>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var response = await _useCase.ExecuteAsync(
            new ResendInvitationRequest(user.Id), CancellationToken.None);

        response.UserId.Should().Be(user.Id);
        response.Email.Should().Be(user.Email);
        oldInvitation.ConsumedAtUtc.Should().NotBeNull(); // Deve estar consumido
        await _mailService.Received(1)
            .SendAsync(Arg.Is<MailMessage>(m => m.To.Contains(user.Email)), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_DeveEnviarEmailComLinkValido()
    {
        var userId = Guid.CreateVersion7();
        var user = User.Create("Maria", "maria@example.com", "casdoor-456");

        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);
        _invitationRepository.GetPendingByUserAsync(userId, Arg.Any<CancellationToken>())
            .Returns((Invitation?)null);

        await _useCase.ExecuteAsync(
            new ResendInvitationRequest(userId), CancellationToken.None);

        await _mailService.Received(1)
            .SendAsync(Arg.Is<MailMessage>(m =>
                m.Subject.Contains("Novo link") && m.HtmlBody.Contains("https://app.example.com/invite")),
            Arg.Any<CancellationToken>());
    }
}

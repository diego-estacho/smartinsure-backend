using FluentAssertions;
using NSubstitute;
using SmartInsure.Application.UseCase.Services.Invitations;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.ResendInvitation;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.ResendInvitation.Requests;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
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
    private readonly IInvitationMailer _invitationMailer = Substitute.For<IInvitationMailer>();
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
            _invitationRepository, _userRepository, _invitationMailer, _unitOfWork, options);
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

        var response = await _useCase.ExecuteAsync(
            new ResendInvitationRequest(user.Id), CancellationToken.None);

        response.UserId.Should().Be(user.Id);
        response.Email.Should().Be(user.Email);
        oldInvitation.ConsumedAtUtc.Should().NotBeNull(); // Deve estar consumido
        await _invitationMailer.Received(1).SendAsync(
            user.Email, Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
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

        // A composição do link/HTML fica no InvitationMailer; aqui garantimos o reenvio com o assunto certo.
        await _invitationMailer.Received(1).SendAsync(
            user.Email, Arg.Any<string>(), Arg.Any<string>(),
            Arg.Is<string>(subject => subject.Contains("Novo link")), Arg.Any<CancellationToken>());
    }
}

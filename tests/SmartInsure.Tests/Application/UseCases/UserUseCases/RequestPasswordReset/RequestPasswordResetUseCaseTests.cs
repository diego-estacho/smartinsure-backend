using FluentAssertions;
using Microsoft.Extensions.Options;
using NSubstitute;
using SmartInsure.Application.UseCase.Services.Invitations;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.RequestPasswordReset;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.RequestPasswordReset.Requests;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Exceptions;
using SmartInsure.Infra.CrossCutting.Options;
using Xunit;

namespace SmartInsure.Tests.Application.UseCases.UserUseCases.RequestPasswordReset;

/// <summary>RN-203 — redefinição de senha: só Usuário Ativo, gera link e envia e-mail próprio.</summary>
[Trait("Category", "UseCase")]
[Trait("RuleId", "RN-203")]
public sealed class RequestPasswordResetUseCaseTests
{
    private readonly IInvitationRepository _invitationRepository = Substitute.For<IInvitationRepository>();
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IInvitationMailer _invitationMailer = Substitute.For<IInvitationMailer>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly RequestPasswordResetUseCase _useCase;

    public RequestPasswordResetUseCaseTests()
        => _useCase = new RequestPasswordResetUseCase(
            _invitationRepository,
            _userRepository,
            _invitationMailer,
            _unitOfWork,
            Options.Create(new InvitationOptions { AppBaseUrl = "https://app.example.com", LinkExpiryDays = 7 }));

    private User ArrangeActive()
    {
        var user = User.Create("Ana", "ana@corretora.com.br", "casdoor-1");
        user.Activate();
        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        return user;
    }

    [Fact]
    public async Task Execute_DeveGerarLinkEEnviarEmailDeRedefinicao_QuandoAtivo()
    {
        var user = ArrangeActive();
        _invitationRepository.GetPendingByUserAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns((Invitation?)null);

        var result = await _useCase.ExecuteAsync(
            new RequestPasswordResetRequest(user.Id), CancellationToken.None);

        result.Email.Should().Be(user.Email);
        await _invitationRepository.Received(1).AddAsync(Arg.Any<Invitation>(), Arg.Any<CancellationToken>());
        await _invitationMailer.Received(1).SendPasswordResetAsync(
            user.Email, Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received().CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_DeveInvalidarPedidoAnterior_QuandoJaExiste()
    {
        var user = ArrangeActive();
        var previous = Invitation.Create(user.Id, 7).invitation;
        _invitationRepository.GetPendingByUserAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(previous);

        await _useCase.ExecuteAsync(new RequestPasswordResetRequest(user.Id), CancellationToken.None);

        previous.ConsumedAtUtc.Should().NotBeNull();
        await _invitationRepository.Received(1).AddAsync(Arg.Any<Invitation>(), Arg.Any<CancellationToken>());
        await _invitationMailer.Received(1).SendPasswordResetAsync(
            user.Email, Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_DeveRecusar_QuandoPendente()
    {
        var user = User.Create("Beto", "beto@corretora.com.br", "casdoor-2"); // nasce Pendente
        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);

        var act = async () => await _useCase.ExecuteAsync(
            new RequestPasswordResetRequest(user.Id), CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
        await _invitationMailer.DidNotReceiveWithAnyArgs().SendPasswordResetAsync(default!, default!, default!, default);
    }

    [Fact]
    public async Task Execute_DeveRecusar_QuandoInativo()
    {
        var user = User.Create("Cida", "cida@corretora.com.br", "casdoor-3");
        user.Activate();
        user.Deactivate();
        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);

        var act = async () => await _useCase.ExecuteAsync(
            new RequestPasswordResetRequest(user.Id), CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
        await _invitationMailer.DidNotReceiveWithAnyArgs().SendPasswordResetAsync(default!, default!, default!, default);
    }

    [Fact]
    public async Task Execute_DeveLancarNotFound_QuandoUsuarioNaoExiste()
    {
        var id = Guid.NewGuid();
        _userRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((User?)null);

        var act = async () => await _useCase.ExecuteAsync(
            new RequestPasswordResetRequest(id), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}

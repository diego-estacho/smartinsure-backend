using FluentAssertions;
using NSubstitute;
using SmartInsure.Application.UseCase.UseCases.Users.ActivateUser;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Tests.Application.UseCases.Users;

/// <summary>RN-002 — Ativação do Usuário no primeiro acesso.</summary>
[Trait("RuleId", "RN-002")]
public class ActivateUserUseCaseTests
{
    private readonly IUserRepository _repository = Substitute.For<IUserRepository>();
    private readonly IIdentityProvider _identityProvider = Substitute.For<IIdentityProvider>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ActivateUserUseCase _useCase;

    public ActivateUserUseCaseTests()
        => _useCase = new ActivateUserUseCase(_repository, _identityProvider, _unitOfWork);

    private User PendingUser()
    {
        var user = User.Create("Maria Silva", "maria@corretora.com.br", "casdoor-id-123");
        _repository.GetByExternalIdentityAsync("casdoor-id-123", Arg.Any<CancellationToken>())
            .Returns(user);
        return user;
    }

    [Fact]
    public async Task Execute_DeveAtivarUsuario_QuandoTrocaDaSenhaInicialConcluida()
    {
        var user = PendingUser();
        _identityProvider.IsInitialPasswordPendingAsync("casdoor-id-123", Arg.Any<CancellationToken>())
            .Returns(false);

        var response = await _useCase.ExecuteAsync(
            new ActivateUserRequest("casdoor-id-123"), CancellationToken.None);

        response.Status.Should().Be("Active");
        user.Status.Should().Be(EUserStatus.Active);
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_DeveManterPendente_QuandoSenhaInicialAindaNaoTrocada()
    {
        var user = PendingUser();
        _identityProvider.IsInitialPasswordPendingAsync("casdoor-id-123", Arg.Any<CancellationToken>())
            .Returns(true);

        var act = () => _useCase.ExecuteAsync(
            new ActivateUserRequest("casdoor-id-123"), CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
        user.Status.Should().Be(EUserStatus.Pending);
        await _unitOfWork.DidNotReceiveWithAnyArgs().CommitAsync(default);
    }

    [Fact]
    public async Task Execute_DevePermanecerAtivoSemNovaTransicao_QuandoUsuarioJaAtivo()
    {
        var user = PendingUser();
        user.Activate();

        var response = await _useCase.ExecuteAsync(
            new ActivateUserRequest("casdoor-id-123"), CancellationToken.None);

        response.Status.Should().Be("Active");
        await _unitOfWork.DidNotReceiveWithAnyArgs().CommitAsync(default);
    }

    [Fact]
    public async Task Execute_DeveLancarNotFound_QuandoUsuarioNaoExisteNaPlataforma()
    {
        var act = () => _useCase.ExecuteAsync(
            new ActivateUserRequest("desconhecido"), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}

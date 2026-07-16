using FluentAssertions;
using NSubstitute;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.AuthenticateUser;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.AuthenticateUser.Requests;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Tests.Application.UseCases.UserUseCases.AuthenticateUser;

/// <summary>RN-005 — Autenticação de Usuário com e-mail e senha.</summary>
[Trait("RuleId", "RN-005")]
public class AuthenticateUserUseCaseTests
{
    private const string Email = "maria@corretora.com.br";
    private const string Password = "senha-secreta";

    private static readonly DateTime ExpiresAtUtc = new(2026, 7, 16, 20, 0, 0, DateTimeKind.Utc);

    private readonly IUserRepository _repository = Substitute.For<IUserRepository>();
    private readonly IIdentityProvider _identityProvider = Substitute.For<IIdentityProvider>();
    private readonly IAccessTokenIssuer _tokenIssuer = Substitute.For<IAccessTokenIssuer>();
    private readonly AuthenticateUserUseCase _useCase;

    public AuthenticateUserUseCaseTests()
        => _useCase = new AuthenticateUserUseCase(_repository, _identityProvider, _tokenIssuer);

    private User ActiveUser()
    {
        var user = User.Create("Maria Silva", Email, "casdoor-id-123");
        user.Activate();
        _repository.GetByEmailAsync(Email, Arg.Any<CancellationToken>()).Returns(user);
        return user;
    }

    [Fact]
    public async Task Execute_DeveConcederAcesso_QuandoUsuarioAtivoComCredenciaisValidas()
    {
        var user = ActiveUser();
        _identityProvider.ValidateCredentialsAsync(Email, Password, Arg.Any<CancellationToken>())
            .Returns(true);
        _tokenIssuer.IssueFor(user).Returns(new AccessToken("token-plataforma", ExpiresAtUtc));

        var response = await _useCase.ExecuteAsync(
            new AuthenticateUserRequest(Email, Password), CancellationToken.None);

        response.AccessToken.Should().Be("token-plataforma");
        response.ExpiresAtUtc.Should().Be(ExpiresAtUtc);
    }

    [Fact]
    public async Task Execute_DeveNormalizarEmail_QuandoInformadoComMaiusculasEEspacos()
    {
        ActiveUser();
        _identityProvider.ValidateCredentialsAsync(Email, Password, Arg.Any<CancellationToken>())
            .Returns(true);
        _tokenIssuer.IssueFor(Arg.Any<User>()).Returns(new AccessToken("token", ExpiresAtUtc));

        await _useCase.ExecuteAsync(
            new AuthenticateUserRequest($"  {Email.ToUpperInvariant()}  ", Password),
            CancellationToken.None);

        await _repository.Received(1).GetByEmailAsync(Email, Arg.Any<CancellationToken>());
        await _identityProvider.Received(1)
            .ValidateCredentialsAsync(Email, Password, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_DeveRecusarComMensagemGenerica_QuandoUsuarioNaoExisteNaPlataforma()
    {
        _repository.GetByEmailAsync(Email, Arg.Any<CancellationToken>()).Returns((User?)null);

        var act = () => _useCase.ExecuteAsync(
            new AuthenticateUserRequest(Email, Password), CancellationToken.None);

        (await act.Should().ThrowAsync<UnauthorizedException>())
            .WithMessage(AuthenticateUserUseCase.InvalidCredentialsMessage);
    }

    [Fact]
    public async Task Execute_DeveRecusarComMensagemGenerica_QuandoSenhaIncorreta()
    {
        ActiveUser();
        _identityProvider.ValidateCredentialsAsync(Email, Password, Arg.Any<CancellationToken>())
            .Returns(false);

        var act = () => _useCase.ExecuteAsync(
            new AuthenticateUserRequest(Email, Password), CancellationToken.None);

        (await act.Should().ThrowAsync<UnauthorizedException>())
            .WithMessage(AuthenticateUserUseCase.InvalidCredentialsMessage);
        _tokenIssuer.DidNotReceive().IssueFor(Arg.Any<User>());
    }

    [Fact]
    public async Task Execute_DeveRecusar_QuandoUsuarioPendente()
    {
        var user = User.Create("Maria Silva", Email, "casdoor-id-123");
        _repository.GetByEmailAsync(Email, Arg.Any<CancellationToken>()).Returns(user);
        _identityProvider.ValidateCredentialsAsync(Email, Password, Arg.Any<CancellationToken>())
            .Returns(true);

        var act = () => _useCase.ExecuteAsync(
            new AuthenticateUserRequest(Email, Password), CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
        _tokenIssuer.DidNotReceive().IssueFor(Arg.Any<User>());
    }

    [Fact]
    public async Task Execute_DeveRecusarComMensagemGenerica_QuandoUsuarioPendenteComSenhaIncorreta()
    {
        var user = User.Create("Maria Silva", Email, "casdoor-id-123");
        _repository.GetByEmailAsync(Email, Arg.Any<CancellationToken>()).Returns(user);
        _identityProvider.ValidateCredentialsAsync(Email, Password, Arg.Any<CancellationToken>())
            .Returns(false);

        var act = () => _useCase.ExecuteAsync(
            new AuthenticateUserRequest(Email, Password), CancellationToken.None);

        (await act.Should().ThrowAsync<UnauthorizedException>())
            .WithMessage(AuthenticateUserUseCase.InvalidCredentialsMessage);
        _tokenIssuer.DidNotReceive().IssueFor(Arg.Any<User>());
    }

    [Fact]
    public async Task Execute_DevePropagarIndisponibilidade_QuandoProvedorDeIdentidadeFora()
    {
        ActiveUser();
        _identityProvider.ValidateCredentialsAsync(Email, Password, Arg.Any<CancellationToken>())
            .Returns<bool>(_ => throw new IdentityProviderUnavailableException(
                "Provedor de identidade indisponível."));

        var act = () => _useCase.ExecuteAsync(
            new AuthenticateUserRequest(Email, Password), CancellationToken.None);

        await act.Should().ThrowAsync<IdentityProviderUnavailableException>();
        _tokenIssuer.DidNotReceive().IssueFor(Arg.Any<User>());
    }
}

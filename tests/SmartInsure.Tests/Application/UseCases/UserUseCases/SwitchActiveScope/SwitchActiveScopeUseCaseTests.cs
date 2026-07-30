using FluentAssertions;
using NSubstitute;
using SmartInsure.Application.UseCase.Services.Scopes;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.SwitchActiveScope;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.SwitchActiveScope.Requests;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Exceptions;
using Xunit;

namespace SmartInsure.Tests.Application.UseCases.UserUseCases.SwitchActiveScope;

/// <summary>RN-064 — troca da Corretora ativa / do Tomador ativo, com reemissão do acesso.</summary>
[Trait("Category", "UseCase")]
[Trait("RuleId", "RN-064")]
public sealed class SwitchActiveScopeUseCaseTests
{
    private const string ExternalIdentity = "casdoor-id-123";

    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IActiveScopeResolver _resolver = Substitute.For<IActiveScopeResolver>();
    private readonly IAccessTokenIssuer _tokenIssuer = Substitute.For<IAccessTokenIssuer>();
    private readonly SwitchActiveScopeUseCase _useCase;

    public SwitchActiveScopeUseCaseTests()
        => _useCase = new SwitchActiveScopeUseCase(_userRepository, _resolver, _tokenIssuer);

    private User ActiveUser()
    {
        var user = User.Create("Maria Silva", "maria@corretora.com.br", ExternalIdentity);
        user.Activate();
        _userRepository.GetByExternalIdentityAsync(ExternalIdentity, Arg.Any<CancellationToken>())
            .Returns(user);
        return user;
    }

    [Fact]
    public async Task Execute_DeveReemitirAcessoComNovoEscopo()
    {
        var user = ActiveUser();
        var brokerageId = Guid.NewGuid();
        var expiresAtUtc = DateTime.UtcNow.AddHours(8);
        _resolver.ResolveRequestedAsync(user.Id, brokerageId, null, Arg.Any<CancellationToken>())
            .Returns(new ActiveScope(brokerageId, null));
        _tokenIssuer.IssueFor(user, Arg.Any<ActiveScope>())
            .Returns(new AccessToken("token-novo", expiresAtUtc));

        var response = await _useCase.ExecuteAsync(
            new SwitchActiveScopeRequest(ExternalIdentity, brokerageId, null),
            CancellationToken.None);

        response.AccessToken.Should().Be("token-novo");
        response.ActiveBrokerageId.Should().Be(brokerageId);
        response.ExpiresAtUtc.Should().Be(expiresAtUtc);
    }

    [Fact]
    public async Task Execute_DeveRecusarUsuarioNaoAtivo()
    {
        var pending = User.Create("Bruno", "bruno@corretora.com.br", ExternalIdentity);
        _userRepository.GetByExternalIdentityAsync(ExternalIdentity, Arg.Any<CancellationToken>())
            .Returns(pending);

        var act = async () => await _useCase.ExecuteAsync(
            new SwitchActiveScopeRequest(ExternalIdentity, Guid.NewGuid(), null),
            CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
        _tokenIssuer.DidNotReceiveWithAnyArgs().IssueFor(default!, default!);
    }

    [Fact]
    public async Task Execute_DeveLancarNotFound_QuandoIdentidadeDesconhecida()
    {
        _userRepository.GetByExternalIdentityAsync(ExternalIdentity, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var act = async () => await _useCase.ExecuteAsync(
            new SwitchActiveScopeRequest(ExternalIdentity, null, null), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Execute_DevePropagarRecusaDoResolver_QuandoNaoHaVinculo()
    {
        var user = ActiveUser();
        var brokerageId = Guid.NewGuid();
        _resolver.ResolveRequestedAsync(user.Id, brokerageId, null, Arg.Any<CancellationToken>())
            .Returns<ActiveScope>(_ =>
                throw new BusinessRuleException("O usuário não está vinculado a esta corretora."));

        var act = async () => await _useCase.ExecuteAsync(
            new SwitchActiveScopeRequest(ExternalIdentity, brokerageId, null),
            CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
        _tokenIssuer.DidNotReceiveWithAnyArgs().IssueFor(default!, default!);
    }
}

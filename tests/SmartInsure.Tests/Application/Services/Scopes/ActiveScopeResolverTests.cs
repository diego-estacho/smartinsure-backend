using FluentAssertions;
using NSubstitute;
using SmartInsure.Application.UseCase.Services.Scopes;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Exceptions;
using Xunit;

namespace SmartInsure.Tests.Application.Services.Scopes;

/// <summary>RN-064 — Escopo ativo derivado dos Vínculos do Usuário.</summary>
[Trait("Category", "Service")]
[Trait("RuleId", "RN-064")]
public sealed class ActiveScopeResolverTests
{
    private readonly IUserBrokerageMembershipRepository _brokerageMemberships =
        Substitute.For<IUserBrokerageMembershipRepository>();

    private readonly IUserPolicyHolderMembershipRepository _policyHolderMemberships =
        Substitute.For<IUserPolicyHolderMembershipRepository>();

    private readonly ActiveScopeResolver _resolver;

    public ActiveScopeResolverTests()
        => _resolver = new ActiveScopeResolver(_brokerageMemberships, _policyHolderMemberships);

    [Fact]
    public async Task ResolveDefault_DeveAtivarVinculoUnico_SemEscolhaDoUsuario()
    {
        var userId = Guid.NewGuid();
        var brokerageId = Guid.NewGuid();
        var profileId = Guid.NewGuid();
        _brokerageMemberships.GetByUserAsync(userId, Arg.Any<CancellationToken>())
            .Returns([UserBrokerageMembership.Create(userId, brokerageId, profileId)]);
        _policyHolderMemberships.GetByUserAsync(userId, Arg.Any<CancellationToken>())
            .Returns([]);

        var scope = await _resolver.ResolveDefaultAsync(userId, CancellationToken.None);

        scope.BrokerageId.Should().Be(brokerageId);
        scope.PolicyHolderId.Should().BeNull();
    }

    [Fact]
    public async Task ResolveDefault_NaoDeveEscolherPorContaPropria_QuandoHaMaisDeUmVinculo()
    {
        var userId = Guid.NewGuid();
        var profileId = Guid.NewGuid();
        _brokerageMemberships.GetByUserAsync(userId, Arg.Any<CancellationToken>())
            .Returns(
            [
                UserBrokerageMembership.Create(userId, Guid.NewGuid(), profileId),
                UserBrokerageMembership.Create(userId, Guid.NewGuid(), profileId),
            ]);
        _policyHolderMemberships.GetByUserAsync(userId, Arg.Any<CancellationToken>())
            .Returns([]);

        var scope = await _resolver.ResolveDefaultAsync(userId, CancellationToken.None);

        scope.BrokerageId.Should().BeNull();
    }

    [Fact]
    public async Task ResolveDefault_DeveFicarSemEscopo_QuandoUsuarioNaoTemVinculo()
    {
        var userId = Guid.NewGuid();
        _brokerageMemberships.GetByUserAsync(userId, Arg.Any<CancellationToken>()).Returns([]);
        _policyHolderMemberships.GetByUserAsync(userId, Arg.Any<CancellationToken>()).Returns([]);

        var scope = await _resolver.ResolveDefaultAsync(userId, CancellationToken.None);

        scope.BrokerageId.Should().BeNull();
        scope.PolicyHolderId.Should().BeNull();
    }

    [Fact]
    public async Task ResolveRequested_DeveAceitarEscopoComVinculo()
    {
        var userId = Guid.NewGuid();
        var brokerageId = Guid.NewGuid();
        _brokerageMemberships.ExistsAsync(userId, brokerageId, Arg.Any<CancellationToken>())
            .Returns(true);

        var scope = await _resolver.ResolveRequestedAsync(
            userId, brokerageId, null, CancellationToken.None);

        scope.BrokerageId.Should().Be(brokerageId);
    }

    [Fact]
    public async Task ResolveRequested_DeveRecusarCorretoraSemVinculo()
    {
        var userId = Guid.NewGuid();
        var brokerageId = Guid.NewGuid();
        _brokerageMemberships.ExistsAsync(userId, brokerageId, Arg.Any<CancellationToken>())
            .Returns(false);

        var act = async () => await _resolver.ResolveRequestedAsync(
            userId, brokerageId, null, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    public async Task ResolveRequested_DeveRecusarTomadorSemVinculo()
    {
        var userId = Guid.NewGuid();
        var policyHolderId = Guid.NewGuid();
        _policyHolderMemberships.ExistsAsync(userId, policyHolderId, Arg.Any<CancellationToken>())
            .Returns(false);

        var act = async () => await _resolver.ResolveRequestedAsync(
            userId, null, policyHolderId, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    public async Task ResolveRequested_DevePermitirSairDoEscopo()
    {
        var scope = await _resolver.ResolveRequestedAsync(
            Guid.NewGuid(), null, null, CancellationToken.None);

        scope.Should().Be(SmartInsure.Core.Abstractions.Services.ActiveScope.None);
    }
}

using FluentAssertions;
using NSubstitute;
using SmartInsure.Application.UseCase.Services.Scopes;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Constants;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;
using Xunit;

namespace SmartInsure.Tests.Application.Services.Scopes;

/// <summary>RN-068/RN-069/RN-070 — quem administra o Escopo ativo.</summary>
[Trait("Category", "Service")]
[Trait("RuleId", "RN-069")]
public sealed class ScopeAuthorizationTests
{
    private const string ExternalIdentity = "casdoor-ca-1";

    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IProfileRepository _profileRepository = Substitute.For<IProfileRepository>();
    private readonly IUserBrokerageMembershipRepository _brokerageMemberships =
        Substitute.For<IUserBrokerageMembershipRepository>();

    private readonly IUserPolicyHolderMembershipRepository _policyHolderMemberships =
        Substitute.For<IUserPolicyHolderMembershipRepository>();

    private readonly ScopeAuthorization _authorization;
    private readonly Guid _brokerageId = Guid.NewGuid();

    public ScopeAuthorizationTests()
        => _authorization = new ScopeAuthorization(
            _userRepository, _profileRepository, _brokerageMemberships, _policyHolderMemberships);

    private User ArrangeActiveUser()
    {
        var user = User.Create("Carla CA", "carla@corretora.com.br", ExternalIdentity);
        user.Activate();
        _userRepository.GetByExternalIdentityAsync(ExternalIdentity, Arg.Any<CancellationToken>())
            .Returns(user);
        return user;
    }

    private Profile ArrangeBrokerageAdministratorProfile()
    {
        var profile = Profile.Create(ProfileNames.BrokerageAdministrator, EProfileScope.Brokerage, true);
        _profileRepository.GetBrokerageAdministratorAsync(Arg.Any<CancellationToken>()).Returns(profile);
        return profile;
    }

    [Fact]
    public async Task RequireBrokerageAdministrator_DeveAutorizar_QuandoPerfilNaCorretoraAtivaEhCA()
    {
        var user = ArrangeActiveUser();
        var profile = ArrangeBrokerageAdministratorProfile();
        _brokerageMemberships.GetByUserAndBrokerageAsync(
            user.Id, _brokerageId, Arg.Any<CancellationToken>())
            .Returns(UserBrokerageMembership.Create(user.Id, _brokerageId, profile.Id));

        var actor = await _authorization.RequireBrokerageAdministratorAsync(
            ExternalIdentity, _brokerageId, CancellationToken.None);

        actor.ScopeId.Should().Be(_brokerageId);
        actor.User.Id.Should().Be(user.Id);
    }

    [Fact]
    public async Task RequireBrokerageAdministrator_DeveRecusar_QuandoNaoHaCorretoraAtiva()
    {
        ArrangeActiveUser();

        var act = async () => await _authorization.RequireBrokerageAdministratorAsync(
            ExternalIdentity, null, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    public async Task RequireBrokerageAdministrator_DeveRecusar_QuandoPerfilNaCorretoraNaoEhCA()
    {
        var user = ArrangeActiveUser();
        ArrangeBrokerageAdministratorProfile();
        var outroPerfil = Profile.Create(ProfileNames.BrokerageUser, EProfileScope.Brokerage, true);
        _brokerageMemberships.GetByUserAndBrokerageAsync(
            user.Id, _brokerageId, Arg.Any<CancellationToken>())
            .Returns(UserBrokerageMembership.Create(user.Id, _brokerageId, outroPerfil.Id));

        var act = async () => await _authorization.RequireBrokerageAdministratorAsync(
            ExternalIdentity, _brokerageId, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task RequireBrokerageAdministrator_DeveRecusar_QuandoNaoHaVinculoComACorretora()
    {
        var user = ArrangeActiveUser();
        ArrangeBrokerageAdministratorProfile();
        _brokerageMemberships.GetByUserAndBrokerageAsync(
            user.Id, _brokerageId, Arg.Any<CancellationToken>())
            .Returns((UserBrokerageMembership?)null);

        var act = async () => await _authorization.RequireBrokerageAdministratorAsync(
            ExternalIdentity, _brokerageId, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task RequireBrokerageAdministrator_DeveRecusarUsuarioNaoAtivo()
    {
        var pending = User.Create("Bruno", "bruno@corretora.com.br", ExternalIdentity);
        _userRepository.GetByExternalIdentityAsync(ExternalIdentity, Arg.Any<CancellationToken>())
            .Returns(pending);

        var act = async () => await _authorization.RequireBrokerageAdministratorAsync(
            ExternalIdentity, _brokerageId, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    [Trait("RuleId", "RN-070")]
    public async Task RequirePolicyHolderAdministrator_DeveAutorizar_QuandoPerfilNoTomadorAtivoEhTA()
    {
        var user = ArrangeActiveUser();
        var policyHolderId = Guid.NewGuid();
        var profile = Profile.Create(
            ProfileNames.PolicyHolderAdministrator, EProfileScope.PolicyHolder, true);
        _profileRepository.GetByNameAsync(
            ProfileNames.PolicyHolderAdministrator, Arg.Any<CancellationToken>())
            .Returns(profile);
        _policyHolderMemberships.GetByUserAndPolicyHolderAsync(
            user.Id, policyHolderId, Arg.Any<CancellationToken>())
            .Returns(UserPolicyHolderMembership.Create(user.Id, policyHolderId, profile.Id));

        var actor = await _authorization.RequirePolicyHolderAdministratorAsync(
            ExternalIdentity, policyHolderId, CancellationToken.None);

        actor.ScopeId.Should().Be(policyHolderId);
    }
}

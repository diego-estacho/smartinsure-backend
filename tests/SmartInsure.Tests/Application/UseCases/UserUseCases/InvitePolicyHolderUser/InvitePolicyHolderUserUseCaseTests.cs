using FluentAssertions;
using NSubstitute;
using SmartInsure.Application.UseCase.Services.Invitations;
using SmartInsure.Application.UseCase.Services.Scopes;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.InvitePolicyHolderUser;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.InvitePolicyHolderUser.Requests;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Constants;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;
using Xunit;

namespace SmartInsure.Tests.Application.UseCases.UserUseCases.InvitePolicyHolderUser;

/// <summary>RN-070 — o Tomador Administrador cria Usuários do Tomador ativo.</summary>
[Trait("Category", "UseCase")]
[Trait("RuleId", "RN-070")]
public sealed class InvitePolicyHolderUserUseCaseTests
{
    private const string Identity = "casdoor-ta";

    private readonly IScopeAuthorization _scopeAuthorization = Substitute.For<IScopeAuthorization>();
    private readonly IProfileRepository _profileRepository = Substitute.For<IProfileRepository>();
    private readonly IInvitedUserService _invitedUserService = Substitute.For<IInvitedUserService>();
    private readonly InvitePolicyHolderUserUseCase _useCase;

    private readonly Guid _policyHolderId = Guid.NewGuid();

    public InvitePolicyHolderUserUseCaseTests()
    {
        _useCase = new InvitePolicyHolderUserUseCase(
            _scopeAuthorization, _profileRepository, _invitedUserService);

        var actor = User.Create("Tina TA", "tina@tomador.com.br", Identity);
        actor.Activate();
        _scopeAuthorization.RequirePolicyHolderAdministratorAsync(
            Identity, _policyHolderId, Arg.Any<CancellationToken>())
            .Returns(new ScopeActor(actor, _policyHolderId));

        _invitedUserService.InviteAsync(Arg.Any<InviteUserCommand>(), Arg.Any<CancellationToken>())
            .Returns(User.Create("Novo Tomador", "usuario@tomador.com.br", "casdoor-novo"));
    }

    private void ArrangeProfile(Profile profile)
        => _profileRepository.GetByIdAsync(profile.Id, Arg.Any<CancellationToken>()).Returns(profile);

    private InvitePolicyHolderUserRequest Request(Guid profileId)
        => new(Identity, _policyHolderId, "Novo Tomador", "usuario@tomador.com.br", profileId);

    [Fact]
    public async Task Execute_DeveConvidarComVinculoNoTomadorAtivo_QuandoPerfilFixoTomador()
    {
        var profile = Profile.Create(ProfileNames.PolicyHolderUser, EProfileScope.PolicyHolder, true);
        ArrangeProfile(profile);

        var response = await _useCase.ExecuteAsync(Request(profile.Id), CancellationToken.None);

        response.PolicyHolderId.Should().Be(_policyHolderId);
        response.ProfileName.Should().Be(ProfileNames.PolicyHolderUser);
        response.Status.Should().Be(nameof(EUserStatus.Pending));
        await _invitedUserService.Received(1).InviteAsync(
            Arg.Is<InviteUserCommand>(command =>
                command.BrokerageMemberships.Count == 0
                && command.PolicyHolderMemberships.Count == 1
                && command.PolicyHolderMemberships.Single().ScopeId == _policyHolderId
                && command.PolicyHolderMemberships.Single().ProfileId == profile.Id),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_DeveAceitarPerfilCustomizadoDoProprioTomador()
    {
        var profile = Profile.CreateForPolicyHolder("Financeiro", _policyHolderId);
        ArrangeProfile(profile);

        var response = await _useCase.ExecuteAsync(Request(profile.Id), CancellationToken.None);

        response.ProfileName.Should().Be("Financeiro");
    }

    [Fact]
    public async Task Execute_DeveRecusarPerfilCustomizadoDeOutroTomador()
    {
        var profile = Profile.CreateForPolicyHolder("Financeiro", Guid.NewGuid());
        ArrangeProfile(profile);

        var act = async () => await _useCase.ExecuteAsync(Request(profile.Id), CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
        await _invitedUserService.DidNotReceiveWithAnyArgs().InviteAsync(default!, default);
    }

    [Fact]
    public async Task Execute_DeveRecusarPerfilDeOutroEscopo()
    {
        var profile = Profile.Create(ProfileNames.BrokerageUser, EProfileScope.Brokerage, true);
        ArrangeProfile(profile);

        var act = async () => await _useCase.ExecuteAsync(Request(profile.Id), CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    [Trait("RuleId", "RN-068")]
    public async Task Execute_DeveRecusarPerfilTomadorAdministrador_PorqueQuemConcedeEhOCorretorAdministrador()
    {
        var profile = Profile.Create(
            ProfileNames.PolicyHolderAdministrator, EProfileScope.PolicyHolder, true);
        ArrangeProfile(profile);

        var act = async () => await _useCase.ExecuteAsync(Request(profile.Id), CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
        await _invitedUserService.DidNotReceiveWithAnyArgs().InviteAsync(default!, default);
    }

    [Fact]
    public async Task Execute_DevePropagarRecusaDeAutorizacao_QuandoSolicitanteNaoEhTomadorAdministrador()
    {
        _scopeAuthorization.RequirePolicyHolderAdministratorAsync(
            Identity, _policyHolderId, Arg.Any<CancellationToken>())
            .Returns<ScopeActor>(_ => throw new UnauthorizedException("recusado"));

        var act = async () => await _useCase.ExecuteAsync(
            Request(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }
}

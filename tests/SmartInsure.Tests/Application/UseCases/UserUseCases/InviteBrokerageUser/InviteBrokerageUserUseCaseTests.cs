using FluentAssertions;
using NSubstitute;
using SmartInsure.Application.UseCase.Services.Invitations;
using SmartInsure.Application.UseCase.Services.Scopes;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.InviteBrokerageUser;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.InviteBrokerageUser.Requests;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Constants;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;
using Xunit;

namespace SmartInsure.Tests.Application.UseCases.UserUseCases.InviteBrokerageUser;

/// <summary>RN-069 — o Corretor Administrador cria Usuário na Corretora ativa.</summary>
[Trait("Category", "UseCase")]
[Trait("RuleId", "RN-069")]
public sealed class InviteBrokerageUserUseCaseTests
{
    private const string ExternalIdentity = "casdoor-ca-1";

    private readonly IScopeAuthorization _scopeAuthorization = Substitute.For<IScopeAuthorization>();
    private readonly IProfileRepository _profileRepository = Substitute.For<IProfileRepository>();
    private readonly IInvitedUserService _invitedUserService = Substitute.For<IInvitedUserService>();
    private readonly InviteBrokerageUserUseCase _useCase;

    private readonly Guid _brokerageId = Guid.NewGuid();

    public InviteBrokerageUserUseCaseTests()
    {
        _useCase = new InviteBrokerageUserUseCase(
            _scopeAuthorization, _profileRepository, _invitedUserService);

        var actorUser = User.Create("Carla CA", "carla@corretora.com.br", ExternalIdentity);
        actorUser.Activate();
        _scopeAuthorization.RequireBrokerageAdministratorAsync(
            ExternalIdentity, _brokerageId, Arg.Any<CancellationToken>())
            .Returns(new ScopeActor(actorUser, _brokerageId));

        _invitedUserService.InviteAsync(Arg.Any<InviteUserCommand>(), Arg.Any<CancellationToken>())
            .Returns(User.Create("Novo Corretor", "corretor@corretora.com.br", "casdoor-novo"));
    }

    private void ArrangeProfile(Profile profile)
        => _profileRepository.GetByIdAsync(profile.Id, Arg.Any<CancellationToken>()).Returns(profile);

    private InviteBrokerageUserRequest Request(Guid profileId)
        => new(ExternalIdentity, _brokerageId, "Novo Corretor", "corretor@corretora.com.br", profileId);

    [Fact]
    public async Task Execute_DeveConvidarComVinculoNaCorretoraAtiva_QuandoPerfilFixoCorretor()
    {
        var profile = Profile.Create(ProfileNames.BrokerageUser, EProfileScope.Brokerage, true);
        ArrangeProfile(profile);

        var response = await _useCase.ExecuteAsync(Request(profile.Id), CancellationToken.None);

        response.BrokerageId.Should().Be(_brokerageId);
        response.ProfileName.Should().Be(ProfileNames.BrokerageUser);
        response.Status.Should().Be(nameof(EUserStatus.Pending));
        await _invitedUserService.Received(1).InviteAsync(
            Arg.Is<InviteUserCommand>(command =>
                command.PolicyHolderMemberships.Count == 0
                && command.BrokerageMemberships.Count == 1
                && command.BrokerageMemberships.Single().ScopeId == _brokerageId
                && command.BrokerageMemberships.Single().ProfileId == profile.Id),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_DeveRecusarPerfilDeOutroEscopo()
    {
        var profile = Profile.Create("PerfilDeSistema", EProfileScope.System, false);
        ArrangeProfile(profile);

        var act = async () => await _useCase.ExecuteAsync(Request(profile.Id), CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
        await _invitedUserService.DidNotReceiveWithAnyArgs().InviteAsync(default!, default);
    }

    [Fact]
    public async Task Execute_DeveRecusarPerfilCorretorAdministrador_PorqueQuemConcedeEhOAdministradorDoSistema()
    {
        var profile = Profile.Create(ProfileNames.BrokerageAdministrator, EProfileScope.Brokerage, true);
        ArrangeProfile(profile);

        var act = async () => await _useCase.ExecuteAsync(Request(profile.Id), CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
        await _invitedUserService.DidNotReceiveWithAnyArgs().InviteAsync(default!, default);
    }

    [Fact]
    public async Task Execute_DeveRecusarPerfilCustomizadoDeOutraCorretora()
    {
        // RN-069/RN-072: perfil customizado vale só na Corretora dona dele.
        var deOutraCorretora = Profile.CreateForBrokerage("Operador", Guid.NewGuid());
        ArrangeProfile(deOutraCorretora);

        var act = async () => await _useCase.ExecuteAsync(
            Request(deOutraCorretora.Id), CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
        await _invitedUserService.DidNotReceiveWithAnyArgs().InviteAsync(default!, default);
    }

    [Fact]
    public async Task Execute_DeveLancarNotFound_QuandoPerfilNaoExiste()
    {
        var profileId = Guid.NewGuid();
        _profileRepository.GetByIdAsync(profileId, Arg.Any<CancellationToken>()).Returns((Profile?)null);

        var act = async () => await _useCase.ExecuteAsync(Request(profileId), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Execute_DevePropagarRecusaDeAutorizacao_QuandoSolicitanteNaoEhCorretorAdministrador()
    {
        _scopeAuthorization.RequireBrokerageAdministratorAsync(
            ExternalIdentity, _brokerageId, Arg.Any<CancellationToken>())
            .Returns<ScopeActor>(_ => throw new ForbiddenException("recusado"));

        var act = async () => await _useCase.ExecuteAsync(Request(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
        await _invitedUserService.DidNotReceiveWithAnyArgs().InviteAsync(default!, default);
    }
}

using FluentAssertions;
using NSubstitute;
using SmartInsure.Application.UseCase.Services.Invitations;
using SmartInsure.Application.UseCase.Services.Scopes;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.InvitePolicyHolderAdministrator;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.InvitePolicyHolderAdministrator.Requests;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Repositories.Dtos;
using SmartInsure.Core.Constants;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;
using Xunit;

namespace SmartInsure.Tests.Application.UseCases.UserUseCases.InvitePolicyHolderAdministrator;

/// <summary>RN-068 — o Corretor Administrador cria Tomador Administrador de Tomador nomeado.</summary>
[Trait("Category", "UseCase")]
[Trait("RuleId", "RN-068")]
public sealed class InvitePolicyHolderAdministratorUseCaseTests
{
    private const string ExternalIdentity = "casdoor-ca-1";

    private readonly IScopeAuthorization _scopeAuthorization = Substitute.For<IScopeAuthorization>();
    private readonly IPersonRepository _personRepository = Substitute.For<IPersonRepository>();
    private readonly IPolicyHolderAppointmentRepository _appointmentRepository =
        Substitute.For<IPolicyHolderAppointmentRepository>();

    private readonly IProfileRepository _profileRepository = Substitute.For<IProfileRepository>();
    private readonly IInvitedUserService _invitedUserService = Substitute.For<IInvitedUserService>();
    private readonly InvitePolicyHolderAdministratorUseCase _useCase;

    private readonly Guid _brokerageId = Guid.NewGuid();
    private readonly Guid _policyHolderId = Guid.NewGuid();

    public InvitePolicyHolderAdministratorUseCaseTests()
        => _useCase = new InvitePolicyHolderAdministratorUseCase(
            _scopeAuthorization,
            _personRepository,
            _appointmentRepository,
            _profileRepository,
            _invitedUserService);

    private void ArrangeActor()
    {
        var actorUser = User.Create("Carla CA", "carla@corretora.com.br", ExternalIdentity);
        actorUser.Activate();
        _scopeAuthorization.RequireBrokerageAdministratorAsync(
            ExternalIdentity, _brokerageId, Arg.Any<CancellationToken>())
            .Returns(new ScopeActor(actorUser, _brokerageId));
    }

    private void ArrangePolicyHolder()
        => _personRepository.GetPolicyHolderByIdAsync(_policyHolderId, Arg.Any<CancellationToken>())
            .Returns(new PolicyHolderDetailsDto(
                _policyHolderId, "11222333000181", "Tomador Alfa", null, null, null, null, [], []));

    private Profile ArrangePolicyHolderAdministratorProfile()
    {
        var profile = Profile.Create(ProfileNames.PolicyHolderAdministrator, EProfileScope.PolicyHolder, true);
        _profileRepository.GetByNameAsync(
            ProfileNames.PolicyHolderAdministrator, Arg.Any<CancellationToken>())
            .Returns(profile);
        return profile;
    }

    private InvitePolicyHolderAdministratorRequest Request()
        => new(ExternalIdentity, _brokerageId, "Novo TA", "ta@tomador.com.br", "52998224725", _policyHolderId);

    [Fact]
    public async Task Execute_DeveConvidarComVinculoDeTomador_QuandoHaNomeacaoVigenteNaCorretoraAtiva()
    {
        ArrangeActor();
        ArrangePolicyHolder();
        var profile = ArrangePolicyHolderAdministratorProfile();
        _appointmentRepository.ExistsActiveForPolicyHolderAndBrokerageAsync(
            _policyHolderId, _brokerageId, Arg.Any<CancellationToken>())
            .Returns(true);
        var invited = User.Create("Novo TA", "ta@tomador.com.br", "casdoor-novo");
        _invitedUserService.InviteAsync(Arg.Any<InviteUserCommand>(), Arg.Any<CancellationToken>())
            .Returns(invited);

        var response = await _useCase.ExecuteAsync(Request(), CancellationToken.None);

        response.Status.Should().Be(nameof(EUserStatus.Pending));
        response.PolicyHolderId.Should().Be(_policyHolderId);
        await _invitedUserService.Received(1).InviteAsync(
            Arg.Is<InviteUserCommand>(command =>
                command.BrokerageMemberships.Count == 0
                && command.PolicyHolderMemberships.Count == 1
                && command.PolicyHolderMemberships.Single().ScopeId == _policyHolderId
                && command.PolicyHolderMemberships.Single().ProfileId == profile.Id),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_DeveRecusar_QuandoTomadorNaoTemNomeacaoVigenteNaCorretoraAtiva()
    {
        ArrangeActor();
        ArrangePolicyHolder();
        ArrangePolicyHolderAdministratorProfile();
        _appointmentRepository.ExistsActiveForPolicyHolderAndBrokerageAsync(
            _policyHolderId, _brokerageId, Arg.Any<CancellationToken>())
            .Returns(false);

        var act = async () => await _useCase.ExecuteAsync(Request(), CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
        await _invitedUserService.DidNotReceiveWithAnyArgs()
            .InviteAsync(default!, default);
    }

    [Fact]
    public async Task Execute_DeveLancarNotFound_QuandoTomadorNaoExiste()
    {
        ArrangeActor();
        _personRepository.GetPolicyHolderByIdAsync(_policyHolderId, Arg.Any<CancellationToken>())
            .Returns((PolicyHolderDetailsDto?)null);

        var act = async () => await _useCase.ExecuteAsync(Request(), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
        await _invitedUserService.DidNotReceiveWithAnyArgs().InviteAsync(default!, default);
    }

    [Fact]
    public async Task Execute_DevePropagarRecusaDeAutorizacao_QuandoSolicitanteNaoEhCorretorAdministrador()
    {
        _scopeAuthorization.RequireBrokerageAdministratorAsync(
            ExternalIdentity, _brokerageId, Arg.Any<CancellationToken>())
            .Returns<ScopeActor>(_ => throw new ForbiddenException(
                "Somente o Corretor Administrador da corretora ativa executa esta operação."));

        var act = async () => await _useCase.ExecuteAsync(Request(), CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
        await _invitedUserService.DidNotReceiveWithAnyArgs().InviteAsync(default!, default);
    }
}

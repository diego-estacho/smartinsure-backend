using FluentAssertions;
using NSubstitute;
using SmartInsure.Application.UseCase.Services.Scopes;
using SmartInsure.Application.UseCase.UseCases.ProfileUseCases.CreateScopedProfile;
using SmartInsure.Application.UseCase.UseCases.ProfileUseCases.CreateScopedProfile.Requests;
using SmartInsure.Application.UseCase.UseCases.ProfileUseCases.DeleteScopedProfile;
using SmartInsure.Application.UseCase.UseCases.ProfileUseCases.DeleteScopedProfile.Requests;
using SmartInsure.Application.UseCase.UseCases.ProfileUseCases.UpdateScopedProfile;
using SmartInsure.Application.UseCase.UseCases.ProfileUseCases.UpdateScopedProfile.Requests;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Constants;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;
using Xunit;

namespace SmartInsure.Tests.Application.UseCases.ProfileUseCases.ScopedProfiles;

/// <summary>RN-069/RN-070/RN-074 — Perfis customizados mantidos pelo administrador do Escopo.</summary>
[Trait("Category", "UseCase")]
public sealed class ScopedProfileUseCasesTests
{
    private const string Identity = "casdoor-ca";

    private readonly IScopeAuthorization _scopeAuthorization = Substitute.For<IScopeAuthorization>();
    private readonly IProfileRepository _profileRepository = Substitute.For<IProfileRepository>();
    private readonly IPermissionRepository _permissionRepository = Substitute.For<IPermissionRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private readonly Guid _brokerageId = Guid.NewGuid();
    private readonly User _actor;

    public ScopedProfileUseCasesTests()
    {
        _actor = User.Create("Carla CA", "carla@corretora.com.br", Identity);
        _actor.Activate();
        _scopeAuthorization.RequireScopeAdministratorAsync(
            Identity, _brokerageId, null, Arg.Any<CancellationToken>())
            .Returns(new AdministeredScope(_actor, EProfileScope.Brokerage, _brokerageId));
    }

    private CreateScopedProfileUseCase CreateUseCase()
        => new(_scopeAuthorization, _profileRepository, _permissionRepository, _unitOfWork);

    private UpdateScopedProfileUseCase UpdateUseCase()
        => new(_scopeAuthorization, _profileRepository, _permissionRepository, _unitOfWork);

    private DeleteScopedProfileUseCase DeleteUseCase()
        => new(_scopeAuthorization, _profileRepository, _unitOfWork);

    [Fact]
    [Trait("RuleId", "RN-069")]
    public async Task Create_DeveCriarPerfilVinculadoAoEscopoDoAdministrador()
    {
        var permission = Permission.Create(PermissionCodes.QuotationGroupsView, null, true);
        _permissionRepository.GetByCodesAsync(
            Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
            .Returns([permission]);
        _profileRepository.ExistsByNameInScopeAsync(
            "Operador", EProfileScope.Brokerage, _brokerageId, null, Arg.Any<CancellationToken>())
            .Returns(false);

        var response = await CreateUseCase().ExecuteAsync(
            new CreateScopedProfileRequest(
                Identity, _brokerageId, null, "Operador", [PermissionCodes.QuotationGroupsView]),
            CancellationToken.None);

        response.Scope.Should().Be(nameof(EProfileScope.Brokerage));
        response.BrokerageId.Should().Be(_brokerageId);
        response.PermissionCount.Should().Be(1);
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("RuleId", "RN-069")]
    public async Task Create_DeveRecusarNomeRepetidoNoMesmoEscopo()
    {
        _profileRepository.ExistsByNameInScopeAsync(
            "Operador", EProfileScope.Brokerage, _brokerageId, null, Arg.Any<CancellationToken>())
            .Returns(true);

        var act = async () => await CreateUseCase().ExecuteAsync(
            new CreateScopedProfileRequest(Identity, _brokerageId, null, "Operador", []),
            CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
        await _unitOfWork.DidNotReceiveWithAnyArgs().CommitAsync(default);
    }

    [Fact]
    [Trait("RuleId", "RN-063")]
    public async Task Create_DeveRecusarPermissaoForaDoCatalogo()
    {
        _permissionRepository.GetByCodesAsync(
            Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
            .Returns([]);

        var act = async () => await CreateUseCase().ExecuteAsync(
            new CreateScopedProfileRequest(Identity, _brokerageId, null, "Operador", ["inventada.permissao"]),
            CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
        await _unitOfWork.DidNotReceiveWithAnyArgs().CommitAsync(default);
    }

    [Fact]
    [Trait("RuleId", "RN-062")]
    public async Task Create_DeveAceitarPerfilSemPermissao()
    {
        _profileRepository.ExistsByNameInScopeAsync(
            "Somente leitura", EProfileScope.Brokerage, _brokerageId, null, Arg.Any<CancellationToken>())
            .Returns(false);

        var response = await CreateUseCase().ExecuteAsync(
            new CreateScopedProfileRequest(Identity, _brokerageId, null, "Somente leitura", []),
            CancellationToken.None);

        response.PermissionCount.Should().Be(0);
    }

    [Fact]
    [Trait("RuleId", "RN-074")]
    public async Task Update_DeveRenomearETrocarPermissoes()
    {
        var profile = Profile.CreateForBrokerage("Operador", _brokerageId);
        var antiga = Permission.Create(PermissionCodes.QuotationGroupsView, null, true);
        profile.AddPermission(antiga);
        var nova = Permission.Create(PermissionCodes.PolicyHoldersView, null, true);
        _profileRepository.GetTrackedByIdAsync(profile.Id, Arg.Any<CancellationToken>())
            .Returns(profile);
        _permissionRepository.GetByCodesAsync(
            Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
            .Returns([nova]);
        _profileRepository.ExistsByNameInScopeAsync(
            "Operador Sênior", EProfileScope.Brokerage, _brokerageId, profile.Id, Arg.Any<CancellationToken>())
            .Returns(false);

        var response = await UpdateUseCase().ExecuteAsync(
            new UpdateScopedProfileRequest(
                Identity, _brokerageId, null, profile.Id, "Operador Sênior", [PermissionCodes.PolicyHoldersView]),
            CancellationToken.None);

        response.Name.Should().Be("Operador Sênior");
        // A permissão anterior saiu; ficou só a informada.
        profile.Permissions.Should().ContainSingle()
            .Which.PermissionId.Should().Be(nova.Id);
    }

    [Fact]
    [Trait("RuleId", "RN-073")]
    public async Task Update_DeveRecusarPerfilFixo()
    {
        var fixo = Profile.Create(ProfileNames.BrokerageUser, EProfileScope.Brokerage, true);
        _profileRepository.GetTrackedByIdAsync(fixo.Id, Arg.Any<CancellationToken>()).Returns(fixo);

        var act = async () => await UpdateUseCase().ExecuteAsync(
            new UpdateScopedProfileRequest(Identity, _brokerageId, null, fixo.Id, "Outro nome", []),
            CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    [Trait("RuleId", "RN-074")]
    public async Task Update_DeveRecusarPerfilDeOutraCorretora()
    {
        var deOutra = Profile.CreateForBrokerage("Operador", Guid.NewGuid());
        _profileRepository.GetTrackedByIdAsync(deOutra.Id, Arg.Any<CancellationToken>())
            .Returns(deOutra);

        var act = async () => await UpdateUseCase().ExecuteAsync(
            new UpdateScopedProfileRequest(Identity, _brokerageId, null, deOutra.Id, "Operador", []),
            CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    [Trait("RuleId", "RN-074")]
    public async Task Delete_DeveRemoverPerfilSemUsuarios()
    {
        var profile = Profile.CreateForBrokerage("Operador", _brokerageId);
        _profileRepository.GetTrackedByIdAsync(profile.Id, Arg.Any<CancellationToken>())
            .Returns(profile);
        _profileRepository.CountUsersByProfileAsync(profile.Id, Arg.Any<CancellationToken>())
            .Returns(0);

        await DeleteUseCase().ExecuteAsync(
            new DeleteScopedProfileRequest(Identity, _brokerageId, null, profile.Id),
            CancellationToken.None);

        _profileRepository.Received(1).RemoveWithPermissions(profile);
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("RuleId", "RN-074")]
    public async Task Delete_DeveRecusarPerfilEmUso()
    {
        var profile = Profile.CreateForBrokerage("Operador", _brokerageId);
        _profileRepository.GetTrackedByIdAsync(profile.Id, Arg.Any<CancellationToken>())
            .Returns(profile);
        _profileRepository.CountUsersByProfileAsync(profile.Id, Arg.Any<CancellationToken>())
            .Returns(3);

        var act = async () => await DeleteUseCase().ExecuteAsync(
            new DeleteScopedProfileRequest(Identity, _brokerageId, null, profile.Id),
            CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
        _profileRepository.DidNotReceiveWithAnyArgs().RemoveWithPermissions(default!);
    }

    [Fact]
    [Trait("RuleId", "RN-074")]
    public async Task Delete_DeveRecusarPerfilFixo()
    {
        var fixo = Profile.Create(ProfileNames.BrokerageUser, EProfileScope.Brokerage, true);
        _profileRepository.GetTrackedByIdAsync(fixo.Id, Arg.Any<CancellationToken>()).Returns(fixo);

        var act = async () => await DeleteUseCase().ExecuteAsync(
            new DeleteScopedProfileRequest(Identity, _brokerageId, null, fixo.Id),
            CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    [Trait("RuleId", "RN-074")]
    public async Task Delete_ComUsuarios_DeveMigrarParaODestinoEExcluir()
    {
        var profile = Profile.CreateForBrokerage("Operador", _brokerageId);
        var destino = Profile.CreateForBrokerage("Operador Pleno", _brokerageId);
        _profileRepository.GetTrackedByIdAsync(profile.Id, Arg.Any<CancellationToken>())
            .Returns(profile);
        _profileRepository.GetTrackedByIdAsync(destino.Id, Arg.Any<CancellationToken>())
            .Returns(destino);
        _profileRepository.CountUsersByProfileAsync(profile.Id, Arg.Any<CancellationToken>())
            .Returns(3);

        await DeleteUseCase().ExecuteAsync(
            new DeleteScopedProfileRequest(Identity, _brokerageId, null, profile.Id, destino.Id),
            CancellationToken.None);

        await _profileRepository.Received(1).ReassignMembershipsAsync(
            profile.Id, destino.Id, EProfileScope.Brokerage, Arg.Any<CancellationToken>());
        _profileRepository.Received(1).RemoveWithPermissions(profile);
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("RuleId", "RN-074")]
    public async Task Delete_ComUsuarios_DeveRecusarDestinoDeOutroDono()
    {
        var profile = Profile.CreateForBrokerage("Operador", _brokerageId);
        var destinoDeOutraCorretora = Profile.CreateForBrokerage("Operador", Guid.NewGuid());
        _profileRepository.GetTrackedByIdAsync(profile.Id, Arg.Any<CancellationToken>())
            .Returns(profile);
        _profileRepository.GetTrackedByIdAsync(destinoDeOutraCorretora.Id, Arg.Any<CancellationToken>())
            .Returns(destinoDeOutraCorretora);
        _profileRepository.CountUsersByProfileAsync(profile.Id, Arg.Any<CancellationToken>())
            .Returns(2);

        var act = async () => await DeleteUseCase().ExecuteAsync(
            new DeleteScopedProfileRequest(
                Identity, _brokerageId, null, profile.Id, destinoDeOutraCorretora.Id),
            CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
        await _profileRepository.DidNotReceiveWithAnyArgs()
            .ReassignMembershipsAsync(default, default, default, default);
        _profileRepository.DidNotReceiveWithAnyArgs().RemoveWithPermissions(default!);
    }
}

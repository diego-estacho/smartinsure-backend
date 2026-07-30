using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using SmartInsure.Application.UseCase.UseCases.ProfileUseCases.UpdateFixedProfilePermissions;
using SmartInsure.Application.UseCase.UseCases.ProfileUseCases.UpdateFixedProfilePermissions.Requests;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Constants;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;
using Xunit;

namespace SmartInsure.Tests.Application.UseCases.ProfileUseCases.UpdateFixedProfilePermissions;

/// <summary>RN-073 — o Administrador do Sistema edita as Permissões dos Perfis fixos.</summary>
[Trait("Category", "UseCase")]
[Trait("RuleId", "RN-073")]
public sealed class UpdateFixedProfilePermissionsUseCaseTests
{
    private readonly IProfileRepository _profileRepository = Substitute.For<IProfileRepository>();
    private readonly IPermissionRepository _permissionRepository = Substitute.For<IPermissionRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly UpdateFixedProfilePermissionsUseCase _useCase;

    public UpdateFixedProfilePermissionsUseCaseTests()
        => _useCase = new UpdateFixedProfilePermissionsUseCase(
            _profileRepository,
            _permissionRepository,
            _unitOfWork,
            NullLogger<UpdateFixedProfilePermissionsUseCase>.Instance);

    [Fact]
    public async Task Execute_DeveMarcarPermissoesNoPerfilFixo()
    {
        var profile = Profile.Create(ProfileNames.BrokerageUser, EProfileScope.Brokerage, true);
        var permission = Permission.Create(PermissionCodes.QuotationGroupsView, null, true);
        _profileRepository.GetTrackedByIdAsync(profile.Id, Arg.Any<CancellationToken>())
            .Returns(profile);
        _permissionRepository.GetByCodesAsync(
            Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
            .Returns([permission]);

        var response = await _useCase.ExecuteAsync(
            new UpdateFixedProfilePermissionsRequest(
                profile.Id, [PermissionCodes.QuotationGroupsView]),
            CancellationToken.None);

        response.PermissionCount.Should().Be(1);
        response.Name.Should().Be(ProfileNames.BrokerageUser);
        profile.HasPermission(permission.Id).Should().BeTrue();
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_DeveDesmarcarPermissaoRemovidaDaLista()
    {
        var profile = Profile.Create(ProfileNames.BrokerageUser, EProfileScope.Brokerage, true);
        var anterior = Permission.Create(PermissionCodes.QuotationGroupsView, null, true);
        profile.AddPermission(anterior);
        _profileRepository.GetTrackedByIdAsync(profile.Id, Arg.Any<CancellationToken>())
            .Returns(profile);

        var response = await _useCase.ExecuteAsync(
            new UpdateFixedProfilePermissionsRequest(profile.Id, []),
            CancellationToken.None);

        response.PermissionCount.Should().Be(0);
        profile.HasPermission(anterior.Id).Should().BeFalse();
    }

    // O que a regra prende aqui é o alcance da RN-073 (este fluxo edita apenas Perfil fixo);
    // a edição do customizado é RN-074, exercitada em ScopedProfileUseCasesTests.
    [Fact]
    [Trait("RuleId", "RN-073")]
    public async Task Execute_DeveRecusarPerfilCustomizado_PorqueEleEhEditadoNoProprioEscopo()
    {
        var customizado = Profile.CreateForBrokerage("Operador", Guid.NewGuid());
        _profileRepository.GetTrackedByIdAsync(customizado.Id, Arg.Any<CancellationToken>())
            .Returns(customizado);

        var act = async () => await _useCase.ExecuteAsync(
            new UpdateFixedProfilePermissionsRequest(customizado.Id, []),
            CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
        await _unitOfWork.DidNotReceiveWithAnyArgs().CommitAsync(default);
    }

    [Fact]
    [Trait("RuleId", "RN-063")]
    public async Task Execute_DeveRecusarPermissaoForaDoCatalogo()
    {
        var profile = Profile.Create(ProfileNames.BrokerageUser, EProfileScope.Brokerage, true);
        _profileRepository.GetTrackedByIdAsync(profile.Id, Arg.Any<CancellationToken>())
            .Returns(profile);
        _permissionRepository.GetByCodesAsync(
            Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
            .Returns([]);

        var act = async () => await _useCase.ExecuteAsync(
            new UpdateFixedProfilePermissionsRequest(profile.Id, ["inventada.permissao"]),
            CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    public async Task Execute_DeveLancarNotFound_QuandoPerfilNaoExiste()
    {
        var profileId = Guid.NewGuid();
        _profileRepository.GetTrackedByIdAsync(profileId, Arg.Any<CancellationToken>())
            .Returns((Profile?)null);

        var act = async () => await _useCase.ExecuteAsync(
            new UpdateFixedProfilePermissionsRequest(profileId, []), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}

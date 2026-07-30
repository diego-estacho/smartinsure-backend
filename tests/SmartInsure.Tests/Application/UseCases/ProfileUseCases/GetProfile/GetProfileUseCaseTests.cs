using FluentAssertions;
using NSubstitute;
using SmartInsure.Application.UseCase.Services.Scopes;
using SmartInsure.Application.UseCase.UseCases.ProfileUseCases.GetProfile;
using SmartInsure.Application.UseCase.UseCases.ProfileUseCases.GetProfile.Requests;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Repositories.Dtos;
using SmartInsure.Core.Constants;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;
using Xunit;

namespace SmartInsure.Tests.Application.UseCases.ProfileUseCases.GetProfile;

[Trait("Category", "UseCase")]
public sealed class GetProfileUseCaseTests
{
    private const string AdminIdentity = "casdoor-admin";
    private const string BrokerageAdministratorIdentity = "casdoor-ca";

    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IProfileRepository _profileRepository = Substitute.For<IProfileRepository>();
    private readonly IScopeAuthorization _scopeAuthorization = Substitute.For<IScopeAuthorization>();
    private readonly GetProfileUseCase _useCase;

    private readonly Guid _brokerageId = Guid.NewGuid();

    public GetProfileUseCaseTests()
        => _useCase = new GetProfileUseCase(
            _userRepository, _profileRepository, _scopeAuthorization);

    private void ArrangeSystemAdministrator()
    {
        var admin = User.Create("Admin", "admin@smartinsure.com.br", AdminIdentity);
        admin.Activate();
        admin.GrantProfile(Profile.Create(ProfileNames.SystemAdministrator, EProfileScope.System, true));
        _userRepository.GetByExternalIdentityAsync(AdminIdentity, Arg.Any<CancellationToken>())
            .Returns(admin);
    }

    private void ArrangeBrokerageAdministrator()
    {
        var user = User.Create("Carla CA", "carla@corretora.com.br", BrokerageAdministratorIdentity);
        user.Activate();
        _userRepository.GetByExternalIdentityAsync(
            BrokerageAdministratorIdentity, Arg.Any<CancellationToken>())
            .Returns(user);
        _scopeAuthorization.RequireScopeAdministratorAsync(
            BrokerageAdministratorIdentity, _brokerageId, null, Arg.Any<CancellationToken>())
            .Returns(new AdministeredScope(user, EProfileScope.Brokerage, _brokerageId));
    }

    private GetProfileRequest AdminRequest(Guid profileId)
        => new(profileId, AdminIdentity, null, null);

    private GetProfileRequest BrokerageAdministratorRequest(Guid profileId)
        => new(profileId, BrokerageAdministratorIdentity, _brokerageId, null);

    [Fact]
    [Trait("RuleId", "RN-063")]
    public async Task Execute_DeveTrazerPermissoesMarcadas()
    {
        ArrangeSystemAdministrator();
        var profileId = Guid.NewGuid();
        var permission = new ProfilePermissionDto(
            Guid.NewGuid(), "users.create", "Criar e convidar Usuário", true);
        _profileRepository.GetDetailsByIdAsync(profileId, Arg.Any<CancellationToken>())
            .Returns(new ProfileDetailsDto(
                profileId, ProfileNames.SystemAdministrator, "System", true, null, null, [permission]));

        var result = await _useCase.ExecuteAsync(AdminRequest(profileId), CancellationToken.None);

        result.Permissions.Should().HaveCount(1);
        result.Permissions[0].Code.Should().Be("users.create");
    }

    [Fact]
    [Trait("RuleId", "RN-062")]
    public async Task Execute_DeveDevolverListaVazia_QuandoPerfilSemPermissao()
    {
        ArrangeSystemAdministrator();
        var profileId = Guid.NewGuid();
        _profileRepository.GetDetailsByIdAsync(profileId, Arg.Any<CancellationToken>())
            .Returns(new ProfileDetailsDto(
                profileId, ProfileNames.BrokerageAdministrator, "Brokerage", true, null, null, []));

        var result = await _useCase.ExecuteAsync(AdminRequest(profileId), CancellationToken.None);

        result.Permissions.Should().BeEmpty();
        result.Scope.Should().Be("Brokerage");
    }

    [Fact]
    public async Task Execute_DeveLancarNotFound_QuandoPerfilNaoExiste()
    {
        var profileId = Guid.NewGuid();
        _profileRepository.GetDetailsByIdAsync(profileId, Arg.Any<CancellationToken>())
            .Returns((ProfileDetailsDto?)null);

        var act = async () => await _useCase.ExecuteAsync(
            AdminRequest(profileId), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    [Trait("RuleId", "RN-072")]
    public async Task Execute_DeveEsconderPerfilFixoDeAdministracao_DoCorretorAdministrador()
    {
        ArrangeBrokerageAdministrator();
        var profileId = Guid.NewGuid();
        _profileRepository.GetDetailsByIdAsync(profileId, Arg.Any<CancellationToken>())
            .Returns(new ProfileDetailsDto(
                profileId, ProfileNames.BrokerageAdministrator, "Brokerage", true, null, null, []));

        var act = async () => await _useCase.ExecuteAsync(
            BrokerageAdministratorRequest(profileId), CancellationToken.None);

        // Nem "sem permissão": para ele o perfil não existe (RN-072).
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    [Trait("RuleId", "RN-072")]
    public async Task Execute_DeveRecusarPerfilDeOutraCorretora()
    {
        ArrangeBrokerageAdministrator();
        var profileId = Guid.NewGuid();
        _profileRepository.GetDetailsByIdAsync(profileId, Arg.Any<CancellationToken>())
            .Returns(new ProfileDetailsDto(
                profileId, "Operador", "Brokerage", false, Guid.NewGuid(), null, []));

        var act = async () => await _useCase.ExecuteAsync(
            BrokerageAdministratorRequest(profileId), CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    [Trait("RuleId", "RN-072")]
    public async Task Execute_DevePermitirPerfilCustomizadoDaPropriaCorretora()
    {
        ArrangeBrokerageAdministrator();
        var profileId = Guid.NewGuid();
        _profileRepository.GetDetailsByIdAsync(profileId, Arg.Any<CancellationToken>())
            .Returns(new ProfileDetailsDto(
                profileId, "Operador", "Brokerage", false, _brokerageId, null, []));

        var result = await _useCase.ExecuteAsync(
            BrokerageAdministratorRequest(profileId), CancellationToken.None);

        result.Name.Should().Be("Operador");
        result.IsFixed.Should().BeFalse();
    }
}

using FluentAssertions;
using NSubstitute;
using SmartInsure.Application.UseCase.Services.Scopes;
using SmartInsure.Application.UseCase.UseCases.ProfileUseCases.ListProfiles;
using SmartInsure.Application.UseCase.UseCases.ProfileUseCases.ListProfiles.Requests;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Repositories.Dtos;
using SmartInsure.Core.Constants;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;
using Xunit;

namespace SmartInsure.Tests.Application.UseCases.ProfileUseCases.ListProfiles;

/// <summary>RN-062/RN-072 — gestão de Perfis com visibilidade por Escopo.</summary>
[Trait("Category", "UseCase")]
public sealed class ListProfilesUseCaseTests
{
    private const string AdminIdentity = "casdoor-admin";
    private const string BrokerageAdministratorIdentity = "casdoor-ca";

    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IProfileRepository _profileRepository = Substitute.For<IProfileRepository>();
    private readonly IScopeAuthorization _scopeAuthorization = Substitute.For<IScopeAuthorization>();
    private readonly ListProfilesUseCase _useCase;

    private readonly Guid _brokerageId = Guid.NewGuid();

    public ListProfilesUseCaseTests()
        => _useCase = new ListProfilesUseCase(
            _userRepository, _profileRepository, _scopeAuthorization);

    private void ArrangeSystemAdministrator()
    {
        var admin = User.Create("Admin", "admin@smartinsure.com.br", AdminIdentity);
        admin.Activate();
        admin.GrantProfile(Profile.Create(ProfileNames.SystemAdministrator, EProfileScope.System, true));
        _userRepository.GetByExternalIdentityAsync(AdminIdentity, Arg.Any<CancellationToken>())
            .Returns(admin);
    }

    private User ArrangeBrokerageAdministrator()
    {
        var user = User.Create("Carla CA", "carla@corretora.com.br", BrokerageAdministratorIdentity);
        user.Activate();
        _userRepository.GetByExternalIdentityAsync(
            BrokerageAdministratorIdentity, Arg.Any<CancellationToken>())
            .Returns(user);
        _scopeAuthorization.RequireScopeAdministratorAsync(
            BrokerageAdministratorIdentity, _brokerageId, null, Arg.Any<CancellationToken>())
            .Returns(new AdministeredScope(user, EProfileScope.Brokerage, _brokerageId));
        return user;
    }

    [Fact]
    [Trait("RuleId", "RN-062")]
    public async Task Execute_DeveTrazerTodosOsPerfis_QuandoSolicitanteEhAdministradorDoSistema()
    {
        ArrangeSystemAdministrator();
        var system = new ProfileListItemDto(
            Guid.NewGuid(), ProfileNames.SystemAdministrator, "System", true, null, null, 0);
        var brokerage = new ProfileListItemDto(
            Guid.NewGuid(), ProfileNames.BrokerageAdministrator, "Brokerage", true, null, null, 0);
        _profileRepository.ListAsync(1, 20, null, null, CancellationToken.None)
            .Returns((new[] { system, brokerage }, 2L));

        var result = await _useCase.ExecuteAsync(
            new ListProfilesRequest { ExternalIdentity = AdminIdentity }, CancellationToken.None);

        result.Items.Should().HaveCount(2);
        result.Items[0].Scope.Should().Be("System");
        result.Items[0].IsFixed.Should().BeTrue();
    }

    [Fact]
    [Trait("RuleId", "RN-062")]
    public async Task Execute_DeveFiltrarPorEscopo_QuandoAdministradorDoSistemaInforma()
    {
        ArrangeSystemAdministrator();
        _profileRepository.ListAsync(1, 20, null, EProfileScope.Brokerage, CancellationToken.None)
            .Returns((Array.Empty<ProfileListItemDto>(), 0L));

        await _useCase.ExecuteAsync(
            new ListProfilesRequest { ExternalIdentity = AdminIdentity, Scope = "Brokerage" },
            CancellationToken.None);

        await _profileRepository.Received(1).ListAsync(
            1, 20, null, EProfileScope.Brokerage, CancellationToken.None);
    }

    [Fact]
    public async Task Execute_DeveRecusarEscopoInvalido()
    {
        var act = async () => await _useCase.ExecuteAsync(
            new ListProfilesRequest { ExternalIdentity = AdminIdentity, Scope = "Seguradora" },
            CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    public async Task Execute_DeveSanearPaginacao()
    {
        ArrangeSystemAdministrator();
        _profileRepository.ListAsync(1, 1, null, null, CancellationToken.None)
            .Returns((Array.Empty<ProfileListItemDto>(), 0L));

        var result = await _useCase.ExecuteAsync(
            new ListProfilesRequest { ExternalIdentity = AdminIdentity, Page = -3, PageSize = 0 },
            CancellationToken.None);

        result.Page.Should().Be(1);
        result.PageSize.Should().Be(1);
    }

    [Fact]
    [Trait("RuleId", "RN-072")]
    public async Task Execute_NaoDeveMostrarPerfisFixosDeAdministracao_ParaCorretorAdministrador()
    {
        ArrangeBrokerageAdministrator();
        var brokerageAdministrator = Profile.Create(
            ProfileNames.BrokerageAdministrator, EProfileScope.Brokerage, true);
        var brokerageUser = Profile.Create(ProfileNames.BrokerageUser, EProfileScope.Brokerage, true);
        var customizado = Profile.CreateForBrokerage("Operador", _brokerageId);
        _profileRepository.ListByScopeAsync(
            EProfileScope.Brokerage, _brokerageId, Arg.Any<CancellationToken>())
            .Returns([brokerageAdministrator, brokerageUser, customizado]);

        var result = await _useCase.ExecuteAsync(
            new ListProfilesRequest
            {
                ExternalIdentity = BrokerageAdministratorIdentity,
                ActiveBrokerageId = _brokerageId,
            },
            CancellationToken.None);

        result.Items.Select(item => item.Name).Should()
            .BeEquivalentTo([ProfileNames.BrokerageUser, "Operador"]);
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    [Trait("RuleId", "RN-072")]
    public async Task Execute_DeveRecusar_QuandoSolicitanteNaoAdministraEscopoAlgum()
    {
        var comum = User.Create("Comum", "comum@corretora.com.br", "casdoor-comum");
        comum.Activate();
        _userRepository.GetByExternalIdentityAsync("casdoor-comum", Arg.Any<CancellationToken>())
            .Returns(comum);
        _scopeAuthorization.RequireScopeAdministratorAsync(
            "casdoor-comum", null, null, Arg.Any<CancellationToken>())
            .Returns<AdministeredScope>(_ => throw new BusinessRuleException(
                "Selecione a corretora ou o tomador ativo antes de executar esta operação."));

        var act = async () => await _useCase.ExecuteAsync(
            new ListProfilesRequest { ExternalIdentity = "casdoor-comum" }, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }
}

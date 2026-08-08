using FluentAssertions;
using NSubstitute;
using SmartInsure.Application.UseCase.Services.Scopes;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.ListUsers;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.ListUsers.Requests;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Repositories.Dtos;
using SmartInsure.Core.Constants;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;
using Xunit;

namespace SmartInsure.Tests.Application.UseCases.UserUseCases.ListUsers;

[Trait("Category", "UseCase")]
public sealed class ListUsersUseCaseTests
{
    private const string AdminIdentity = "casdoor-admin";
    private const string BrokerageAdministratorIdentity = "casdoor-ca";

    private static readonly UserStatusCountsDto ZeroCounts = new(0, 0, 0, 0, 0);

    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IScopeAuthorization _scopeAuthorization = Substitute.For<IScopeAuthorization>();
    private readonly ListUsersUseCase _useCase;

    private readonly Guid _brokerageId = Guid.NewGuid();

    public ListUsersUseCaseTests()
        => _useCase = new ListUsersUseCase(_userRepository, _scopeAuthorization);

    /// <summary>Administrador do Sistema: Perfil de Escopo System concedido ao Usuário (RN-012).</summary>
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
        _scopeAuthorization.RequireBrokerageAdministratorAsync(
            BrokerageAdministratorIdentity, _brokerageId, Arg.Any<CancellationToken>())
            .Returns(new ScopeActor(user, _brokerageId));
        return user;
    }

    private void ArrangeEmptyList()
        => _userRepository.ListAsync(
            Arg.Any<int>(), Arg.Any<int>(), Arg.Any<UserListFilters>(), Arg.Any<CancellationToken>())
            .Returns((Array.Empty<UserListItemDto>(), 0L, ZeroCounts));

    [Fact]
    [Trait("RuleId", "RN-012")]
    public async Task Execute_DeveTrazerPerfilEContagensDoUsuario_QuandoHouver()
    {
        ArrangeSystemAdministrator();
        var comPerfil = new UserListItemDto(
            Guid.NewGuid(), "Ana", "ana@exemplo.com", "Active",
            "SystemAdministrator", "System", true, null, DateTime.UtcNow, false, null);
        var semPerfil = new UserListItemDto(
            Guid.NewGuid(), "Bruno", "bruno@exemplo.com", "Pending",
            null, null, false, null, DateTime.UtcNow, false, null);
        _userRepository.ListAsync(1, 20, Arg.Any<UserListFilters>(), CancellationToken.None)
            .Returns((new[] { comPerfil, semPerfil }, 2L, new UserStatusCountsDto(2, 1, 1, 0, 0)));

        var result = await _useCase.ExecuteAsync(
            new ListUsersRequest { ExternalIdentity = AdminIdentity }, CancellationToken.None);

        result.Items.Should().HaveCount(2);
        result.Items[0].ProfileName.Should().Be("SystemAdministrator");
        result.Items[0].ProfileScope.Should().Be("System");
        result.Items[1].ProfileName.Should().BeNull();
        result.TotalCount.Should().Be(2);
        result.Counts.All.Should().Be(2);
        result.Counts.Active.Should().Be(1);
        result.Counts.Pending.Should().Be(1);
    }

    [Fact]
    public async Task Execute_DeveSanearPaginacao()
    {
        ArrangeSystemAdministrator();
        ArrangeEmptyList();

        var result = await _useCase.ExecuteAsync(
            new ListUsersRequest { ExternalIdentity = AdminIdentity, Page = 0, PageSize = 500 },
            CancellationToken.None);

        result.Page.Should().Be(1);
        result.PageSize.Should().Be(100);
        await _userRepository.Received(1).ListAsync(
            1, 100, Arg.Any<UserListFilters>(), CancellationToken.None);
    }

    [Fact]
    public async Task Execute_DeveFiltrarPorSituacao_QuandoInformada()
    {
        ArrangeSystemAdministrator();
        ArrangeEmptyList();

        await _useCase.ExecuteAsync(
            new ListUsersRequest { ExternalIdentity = AdminIdentity, Status = "Inactive" },
            CancellationToken.None);

        await _userRepository.Received(1).ListAsync(
            1, 20,
            Arg.Is<UserListFilters>(filters => filters.Status == EUserListStatusFilter.Inactive),
            CancellationToken.None);
    }

    [Fact]
    [Trait("RuleId", "RN-065")]
    public async Task Execute_DeveMapearExpirado_ParaFiltroDeConviteVencido()
    {
        ArrangeSystemAdministrator();
        ArrangeEmptyList();

        await _useCase.ExecuteAsync(
            new ListUsersRequest { ExternalIdentity = AdminIdentity, Status = "Expired" },
            CancellationToken.None);

        await _userRepository.Received(1).ListAsync(
            1, 20,
            Arg.Is<UserListFilters>(filters => filters.Status == EUserListStatusFilter.Expired),
            CancellationToken.None);
    }

    [Fact]
    public async Task Execute_DeveRepassarFiltrosAvancados_QuandoInformados()
    {
        ArrangeSystemAdministrator();
        ArrangeEmptyList();
        var profileId = Guid.NewGuid();
        var linkId = Guid.NewGuid();

        await _useCase.ExecuteAsync(
            new ListUsersRequest
            {
                ExternalIdentity = AdminIdentity,
                ProfileId = profileId,
                Scope = "Brokerage",
                LinkId = linkId,
            },
            CancellationToken.None);

        await _userRepository.Received(1).ListAsync(
            1, 20,
            Arg.Is<UserListFilters>(filters =>
                filters.ProfileId == profileId
                && filters.Scope == EProfileScope.Brokerage
                && filters.LinkId == linkId),
            CancellationToken.None);
    }

    [Fact]
    public async Task Execute_DeveRecusarSituacaoInvalida()
    {
        ArrangeSystemAdministrator();

        var act = async () => await _useCase.ExecuteAsync(
            new ListUsersRequest { ExternalIdentity = AdminIdentity, Status = "Arquivado" },
            CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
        await _userRepository.DidNotReceiveWithAnyArgs()
            .ListAsync(default, default, default!, default);
    }

    [Fact]
    public async Task Execute_DeveRecusarEscopoInvalido()
    {
        ArrangeSystemAdministrator();

        var act = async () => await _useCase.ExecuteAsync(
            new ListUsersRequest { ExternalIdentity = AdminIdentity, Scope = "Galaxia" },
            CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    [Trait("RuleId", "RN-064")]
    public async Task Execute_DeveRestringirAoEscopoDaCorretoraAtiva_QuandoSolicitanteEhCorretorAdministrador()
    {
        ArrangeBrokerageAdministrator();
        ArrangeEmptyList();

        await _useCase.ExecuteAsync(
            new ListUsersRequest
            {
                ExternalIdentity = BrokerageAdministratorIdentity,
                ActiveBrokerageId = _brokerageId,
            },
            CancellationToken.None);

        await _userRepository.Received(1).ListAsync(
            1, 20,
            Arg.Is<UserListFilters>(filters => filters.VisibleBrokerageId == _brokerageId),
            CancellationToken.None);
    }

    [Fact]
    [Trait("RuleId", "RN-064")]
    public async Task Execute_DeveRecusar_QuandoSolicitanteNaoAdministraEscopoAlgum()
    {
        var comum = User.Create("Comum", "comum@corretora.com.br", "casdoor-comum");
        comum.Activate();
        _userRepository.GetByExternalIdentityAsync("casdoor-comum", Arg.Any<CancellationToken>())
            .Returns(comum);

        var act = async () => await _useCase.ExecuteAsync(
            new ListUsersRequest { ExternalIdentity = "casdoor-comum" }, CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
        await _userRepository.DidNotReceiveWithAnyArgs()
            .ListAsync(default, default, default!, default);
    }

    [Fact]
    public async Task Execute_DeveRepassarBusca_QuandoInformada()
    {
        ArrangeSystemAdministrator();
        ArrangeEmptyList();

        await _useCase.ExecuteAsync(
            new ListUsersRequest
            {
                ExternalIdentity = AdminIdentity,
                Page = 2,
                PageSize = 10,
                Search = "ana",
            },
            CancellationToken.None);

        await _userRepository.Received(1).ListAsync(
            2, 10,
            Arg.Is<UserListFilters>(filters => filters.Search == "ana"),
            CancellationToken.None);
    }
}

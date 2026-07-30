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

    [Fact]
    [Trait("RuleId", "RN-012")]
    public async Task Execute_DeveTrazerPerfilDoUsuario_QuandoHouver()
    {
        ArrangeSystemAdministrator();
        var comPerfil = new UserListItemDto(
            Guid.NewGuid(), "Ana", "ana@exemplo.com", "Active", "SystemAdministrator", DateTime.UtcNow);
        var semPerfil = new UserListItemDto(
            Guid.NewGuid(), "Bruno", "bruno@exemplo.com", "Pending", null, DateTime.UtcNow);
        _userRepository.ListAsync(1, 20, null, null, null, null, CancellationToken.None)
            .Returns((new[] { comPerfil, semPerfil }, 2L));

        var result = await _useCase.ExecuteAsync(
            new ListUsersRequest { ExternalIdentity = AdminIdentity }, CancellationToken.None);

        result.Items.Should().HaveCount(2);
        result.Items[0].ProfileName.Should().Be("SystemAdministrator");
        result.Items[1].ProfileName.Should().BeNull();
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Execute_DeveSanearPaginacao()
    {
        ArrangeSystemAdministrator();
        _userRepository.ListAsync(1, 100, null, null, null, null, CancellationToken.None)
            .Returns((Array.Empty<UserListItemDto>(), 0L));

        var result = await _useCase.ExecuteAsync(
            new ListUsersRequest { ExternalIdentity = AdminIdentity, Page = 0, PageSize = 500 },
            CancellationToken.None);

        result.Page.Should().Be(1);
        result.PageSize.Should().Be(100);
    }

    [Fact]
    public async Task Execute_DeveFiltrarPorSituacao_QuandoInformada()
    {
        ArrangeSystemAdministrator();
        _userRepository.ListAsync(
            1, 20, null, EUserStatus.Inactive, null, null, CancellationToken.None)
            .Returns((Array.Empty<UserListItemDto>(), 0L));

        await _useCase.ExecuteAsync(
            new ListUsersRequest { ExternalIdentity = AdminIdentity, Status = "Inactive" },
            CancellationToken.None);

        await _userRepository.Received(1).ListAsync(
            1, 20, null, EUserStatus.Inactive, null, null, CancellationToken.None);
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
            .ListAsync(default, default, default, default, default, default, default);
    }

    [Fact]
    [Trait("RuleId", "RN-064")]
    public async Task Execute_DeveRestringirAoEscopoDaCorretoraAtiva_QuandoSolicitanteEhCorretorAdministrador()
    {
        ArrangeBrokerageAdministrator();
        _userRepository.ListAsync(1, 20, null, null, _brokerageId, null, CancellationToken.None)
            .Returns((Array.Empty<UserListItemDto>(), 0L));

        await _useCase.ExecuteAsync(
            new ListUsersRequest
            {
                ExternalIdentity = BrokerageAdministratorIdentity,
                ActiveBrokerageId = _brokerageId,
            },
            CancellationToken.None);

        await _userRepository.Received(1).ListAsync(
            1, 20, null, null, _brokerageId, null, CancellationToken.None);
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
            .ListAsync(default, default, default, default, default, default, default);
    }

    [Fact]
    public async Task Execute_DeveRepassarBusca_QuandoInformada()
    {
        ArrangeSystemAdministrator();
        _userRepository.ListAsync(2, 10, "ana", null, null, null, CancellationToken.None)
            .Returns((Array.Empty<UserListItemDto>(), 0L));

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
            2, 10, "ana", null, null, null, CancellationToken.None);
    }
}

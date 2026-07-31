using FluentAssertions;
using NSubstitute;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.GetCurrentUserContext;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.GetCurrentUserContext.Requests;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Repositories.Dtos;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Exceptions;
using Xunit;

namespace SmartInsure.Tests.Application.UseCases.UserUseCases.GetCurrentUserContext;

/// <summary>
/// RN-064 — contexto do próprio acesso: onde o Usuário pode operar (Vínculos, com o Perfil de
/// cada um) e onde está operando agora (Escopo ativo).
/// </summary>
[Trait("Category", "UseCase")]
[Trait("RuleId", "RN-064")]
public sealed class GetCurrentUserContextUseCaseTests
{
    private const string ExternalIdentity = "casdoor-id-123";

    private static readonly Guid UserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid BrokerageA = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid BrokerageB = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly Guid PolicyHolder = Guid.Parse("44444444-4444-4444-4444-444444444444");

    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly GetCurrentUserContextUseCase _useCase;

    public GetCurrentUserContextUseCaseTests()
        => _useCase = new GetCurrentUserContextUseCase(_userRepository);

    private void GivenUserWith(
        IReadOnlyList<UserMembershipDto> brokerages,
        IReadOnlyList<UserMembershipDto> policyHolders,
        string? systemProfileName = null)
    {
        var user = User.Create("Maria Silva", "maria@corretora.com.br", ExternalIdentity);
        user.Activate();

        _userRepository.GetByExternalIdentityAsync(ExternalIdentity, Arg.Any<CancellationToken>())
            .Returns(user);
        _userRepository.GetDetailsByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(new UserDetailsDto(
                UserId,
                "Maria Silva",
                "maria@corretora.com.br",
                "Active",
                systemProfileName is null ? null : Guid.NewGuid(),
                systemProfileName,
                DateTime.UtcNow,
                brokerages,
                policyHolders));
    }

    private static UserMembershipDto Membership(Guid scopeId, string name, string profileName)
        => new(Guid.NewGuid(), scopeId, "12345678000190", name, Guid.NewGuid(), profileName);

    [Fact]
    public async Task Execute_DeveRetornarVinculosComOPerfilDeCadaEscopo()
    {
        GivenUserWith(
            [
                Membership(BrokerageA, "Corretora Alfa", "Corretor Administrador"),
                Membership(BrokerageB, "Corretora Beta", "Corretor"),
            ],
            [Membership(PolicyHolder, "Tomador Gama", "Tomador Administrador")]);

        var response = await _useCase.ExecuteAsync(
            new GetCurrentUserContextRequest(ExternalIdentity, null, null), CancellationToken.None);

        response.Brokerages.Should().HaveCount(2);
        response.Brokerages.Should().Contain(scope =>
            scope.Id == BrokerageA
            && scope.Name == "Corretora Alfa"
            && scope.ProfileName == "Corretor Administrador");
        response.PolicyHolders.Should().ContainSingle()
            .Which.ProfileName.Should().Be("Tomador Administrador");
    }

    [Fact]
    public async Task Execute_DeveMarcarComoAtivoApenasOEscopoDoAcessoCorrente()
    {
        GivenUserWith(
            [
                Membership(BrokerageA, "Corretora Alfa", "Corretor Administrador"),
                Membership(BrokerageB, "Corretora Beta", "Corretor"),
            ],
            [Membership(PolicyHolder, "Tomador Gama", "Tomador")]);

        var response = await _useCase.ExecuteAsync(
            new GetCurrentUserContextRequest(ExternalIdentity, BrokerageB, null),
            CancellationToken.None);

        response.ActiveBrokerageId.Should().Be(BrokerageB);
        response.Brokerages.Single(scope => scope.Id == BrokerageB).IsActive.Should().BeTrue();
        response.Brokerages.Single(scope => scope.Id == BrokerageA).IsActive.Should().BeFalse();
        // Escopo ativo de Corretora não marca Tomador — os dois eixos são independentes.
        response.ActivePolicyHolderId.Should().BeNull();
        response.PolicyHolders.Should().OnlyContain(scope => !scope.IsActive);
    }

    /// <summary>Caso limite da RN-064: sem Vínculo, o Usuário só opera no Escopo Sistema.</summary>
    [Fact]
    public async Task Execute_DeveRetornarPerfilDeSistemaSemVinculos_QuandoAdministradorDoSistema()
    {
        GivenUserWith([], [], systemProfileName: "SystemAdministrator");

        var response = await _useCase.ExecuteAsync(
            new GetCurrentUserContextRequest(ExternalIdentity, null, null), CancellationToken.None);

        response.SystemProfileName.Should().Be("SystemAdministrator");
        response.Brokerages.Should().BeEmpty();
        response.PolicyHolders.Should().BeEmpty();
        response.ActiveBrokerageId.Should().BeNull();
        response.ActivePolicyHolderId.Should().BeNull();
    }

    [Fact]
    public async Task Execute_DeveLancarNotFound_QuandoIdentidadeDesconhecida()
    {
        _userRepository.GetByExternalIdentityAsync(ExternalIdentity, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var act = async () => await _useCase.ExecuteAsync(
            new GetCurrentUserContextRequest(ExternalIdentity, null, null), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}

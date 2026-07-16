using FluentAssertions;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Tests.Core.Entities;

/// <summary>RN-001 — invariantes da entidade Usuário.</summary>
[Trait("RuleId", "RN-001")]
public class UserTests
{
    [Fact]
    public void Create_DeveNascerPendente_QuandoDadosValidos()
    {
        var user = User.Create("Maria Silva", "maria@corretora.com.br", "casdoor-id-123");

        user.Status.Should().Be(EUserStatus.Pending);
        user.ExternalIdentity.Should().Be("casdoor-id-123");
    }

    [Fact]
    public void Create_DeveLancarBusinessRule_QuandoSemIdentidadeNoProvedor()
    {
        var act = () => User.Create("Maria Silva", "maria@corretora.com.br", " ");

        act.Should().Throw<BusinessRuleException>();
    }

    [Fact]
    [Trait("RuleId", "RN-012")]
    public void GrantProfile_DeveConcederPerfil_QuandoUsuarioSemPerfil()
    {
        var user = User.Create("Maria Silva", "maria@corretora.com.br", "casdoor-id-123");

        user.GrantProfile(EUserProfile.SystemAdministrator);

        user.Profile.Should().Be(EUserProfile.SystemAdministrator);
    }

    [Fact]
    [Trait("RuleId", "RN-012")]
    public void GrantProfile_DeveRecusar_QuandoUsuarioJaTemOPerfil()
    {
        var user = User.Create("Maria Silva", "maria@corretora.com.br", "casdoor-id-123");
        user.GrantProfile(EUserProfile.SystemAdministrator);

        var act = () => user.GrantProfile(EUserProfile.SystemAdministrator);

        act.Should().Throw<ConflictException>();
    }

    [Fact]
    [Trait("RuleId", "RN-012")]
    public void RevokeProfile_DeveRemoverPerfil_QuandoUsuarioTemPerfil()
    {
        var user = User.Create("Maria Silva", "maria@corretora.com.br", "casdoor-id-123");
        user.GrantProfile(EUserProfile.SystemAdministrator);

        user.RevokeProfile();

        user.Profile.Should().BeNull();
    }

    [Fact]
    [Trait("RuleId", "RN-012")]
    public void RevokeProfile_DeveRecusar_QuandoUsuarioSemPerfil()
    {
        var user = User.Create("Maria Silva", "maria@corretora.com.br", "casdoor-id-123");

        var act = user.RevokeProfile;

        act.Should().Throw<ConflictException>();
    }
}

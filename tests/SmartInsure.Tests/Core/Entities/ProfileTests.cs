using FluentAssertions;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Tests.Core.Entities;

/// <summary>RN-032 — Perfil como conjunto de Permissões com Escopo.</summary>
[Trait("RuleId", "RN-032")]
public class ProfileTests
{
    [Fact]
    public void Create_DeveNascerComEscopoENome_QuandoDadosValidos()
    {
        var profile = Profile.Create("Operador", EProfileScope.Brokerage, isFixed: false);

        profile.Name.Should().Be("Operador");
        profile.Scope.Should().Be(EProfileScope.Brokerage);
        profile.IsFixed.Should().BeFalse();
        profile.Permissions.Should().BeEmpty();
    }

    [Fact]
    public void Create_DeveLancarBusinessRule_QuandoNomeVazio()
    {
        var act = () => Profile.Create("  ", EProfileScope.System, isFixed: true);

        act.Should().Throw<BusinessRuleException>();
    }

    [Fact]
    public void AddPermission_DeveMarcarPermissao_ENaoDuplicar()
    {
        var profile = Profile.Create("Operador", EProfileScope.Brokerage, isFixed: false);
        var permission = Permission.Create("user.create", "Criar usuário", isSystem: true);

        profile.AddPermission(permission);
        profile.AddPermission(permission);

        profile.Permissions.Should().HaveCount(1);
        profile.HasPermission(permission.Id).Should().BeTrue();
    }

    [Fact]
    public void HasPermission_DeveSerFalso_QuandoPerfilSemAPermissao()
    {
        var profile = Profile.Create("Operador", EProfileScope.Brokerage, isFixed: false);

        profile.HasPermission(Guid.NewGuid()).Should().BeFalse();
    }
}

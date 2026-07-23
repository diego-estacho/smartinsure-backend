using FluentAssertions;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Tests.Core.Entities;

/// <summary>Invariantes da entidade Permissão (catálogo — base da RN-033).</summary>
public class PermissionTests
{
    [Fact]
    public void Create_DeveNormalizarCodigo_QuandoValido()
    {
        var permission = Permission.Create("  user.create  ", "Criar usuário", isSystem: true);

        permission.Code.Should().Be("user.create");
        permission.Description.Should().Be("Criar usuário");
        permission.IsSystem.Should().BeTrue();
    }

    [Fact]
    public void Create_DeveLancarBusinessRule_QuandoCodigoVazio()
    {
        var act = () => Permission.Create("  ", null, isSystem: true);

        act.Should().Throw<BusinessRuleException>();
    }
}

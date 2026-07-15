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
}

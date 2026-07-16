using FluentAssertions;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Tests.Core.Entities;

/// <summary>RN-014/RN-016 — Pessoa com CNPJ canônico e endereço principal único.</summary>
public class PersonTests
{
    [Fact]
    [Trait("RuleId", "RN-014")]
    public void Create_DeveNormalizarCnpjENomes_QuandoDadosValidos()
    {
        var entity = Person.Create(
            "11.444.777/0001-61", "  Alfa Ltda  ", "  ", Guid.NewGuid());

        entity.Cnpj.Should().Be("11444777000161");
        entity.CorporateName.Should().Be("Alfa Ltda");
        entity.TradeName.Should().BeNull();
    }

    [Theory]
    [InlineData("123")]
    [InlineData("")]
    [Trait("RuleId", "RN-014")]
    public void Create_DeveRecusar_QuandoCnpjSemQuatorzeDigitos(string cnpj)
    {
        var action = () => Person.Create(cnpj, "Alfa Ltda", null, Guid.NewGuid());

        action.Should().Throw<BusinessRuleException>();
    }

    [Fact]
    [Trait("RuleId", "RN-014")]
    public void Create_DeveRecusar_QuandoRazaoSocialAusente()
    {
        var action = () => Person.Create("11444777000161", " ", null, Guid.NewGuid());

        action.Should().Throw<BusinessRuleException>();
    }

    [Fact]
    [Trait("RuleId", "RN-014")]
    public void AddMainAddress_DeveRecusarSegundoPrincipal_QuandoJaExiste()
    {
        var entity = Person.Create("11444777000161", "Alfa Ltda", null, Guid.NewGuid());
        entity.AddMainAddress("01310100", "Avenida Paulista", "1000", null, "Bela Vista", "São Paulo", "sp");

        var action = () => entity.AddMainAddress(null, null, null, null, null, null, null);

        action.Should().Throw<ConflictException>();
        entity.Addresses.Should().ContainSingle(address => address.IsMain && address.State == "SP");
    }

    [Theory]
    [InlineData("11444777000161", true)]
    [InlineData("11444777000242", false)]
    [Trait("RuleId", "RN-016")]
    public void IsHeadquarters_DeveIdentificarMatrizPelaOrdemDoCnpj(string cnpj, bool expected)
    {
        var entity = Person.Create(cnpj, "Alfa Ltda", null, Guid.NewGuid());

        entity.IsHeadquarters.Should().Be(expected);
    }
}

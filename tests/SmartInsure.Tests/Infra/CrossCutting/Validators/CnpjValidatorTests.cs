using FluentAssertions;
using SmartInsure.Infra.CrossCutting.Validators;

namespace SmartInsure.Tests.Infra.CrossCutting.Validators;

/// <summary>RN-007 — CNPJ com dígitos verificadores válidos.</summary>
[Trait("RuleId", "RN-007")]
public class CnpjValidatorTests
{
    [Theory]
    [InlineData("12.345.678/0001-95")]
    [InlineData("12345678000195")]
    public void IsValid_DeveAceitar_QuandoDigitosVerificadoresCorretos(string cnpj)
        => CnpjValidator.IsValid(cnpj).Should().BeTrue();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("12345678000190")]
    [InlineData("11111111111111")]
    [InlineData("123")]
    public void IsValid_DeveRecusar_QuandoCnpjInvalido(string? cnpj)
        => CnpjValidator.IsValid(cnpj).Should().BeFalse();
}

/// <summary>RN-016 — resolução da matriz (ordem /0001) a partir de qualquer estabelecimento.</summary>
[Trait("RuleId", "RN-016")]
public class CnpjValidatorHeadquartersTests
{
    [Theory]
    [InlineData("11444777000161", true)]
    [InlineData("11.444.777/0001-61", true)]
    [InlineData("11444777000242", false)]
    [InlineData("123", false)]
    public void IsHeadquarters_DeveIdentificarOrdemMatriz(string cnpj, bool expected)
        => CnpjValidator.IsHeadquarters(cnpj).Should().Be(expected);

    [Theory]
    [InlineData("11444777000242", "11444777000161")]
    [InlineData("11444777000161", "11444777000161")]
    public void HeadquartersOf_DeveResolverMatrizComDigitosRecalculados(string cnpj, string expected)
        => CnpjValidator.HeadquartersOf(cnpj).Should().Be(expected);

    [Fact]
    public void HeadquartersOf_DeveRecusar_QuandoCnpjSemQuatorzeDigitos()
    {
        var action = () => CnpjValidator.HeadquartersOf("123");

        action.Should().Throw<ArgumentException>();
    }
}

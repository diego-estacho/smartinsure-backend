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

using FluentAssertions;
using SmartInsure.Infra.CrossCutting.Validators;

namespace SmartInsure.Tests.Infra.CrossCutting.Validators;

/// <summary>RN-082 — CPF com dígitos verificadores válidos.</summary>
[Trait("RuleId", "RN-082")]
public class CpfValidatorTests
{
    [Theory]
    [InlineData("529.982.247-25")]
    [InlineData("52998224725")]
    [InlineData("111.444.777-35")]
    public void IsValid_DeveAceitar_QuandoDigitosVerificadoresCorretos(string cpf)
        => CpfValidator.IsValid(cpf).Should().BeTrue();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("52998224724")]
    [InlineData("11111111111")]
    [InlineData("123")]
    [InlineData("5299822472")]
    public void IsValid_DeveRecusar_QuandoCpfInvalido(string? cpf)
        => CpfValidator.IsValid(cpf).Should().BeFalse();

    [Fact]
    public void Normalize_DeveManterSomenteDigitos()
        => CpfValidator.Normalize("529.982.247-25").Should().Be("52998224725");
}

using FluentAssertions;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Tests.Core.Entities;

/// <summary>RN-007/RN-009 — criação e transições de situação da Seguradora.</summary>
public class InsurerTests
{
    private static Insurer NewInsurer(EInsurerStatus status = EInsurerStatus.Active)
        => Insurer.Create("12.345.678/0001-95", " Seguradora Alfa S.A. ", null, null, status);

    [Fact]
    [Trait("RuleId", "RN-007")]
    public void Create_DeveNormalizarCnpjEDados_QuandoDadosValidos()
    {
        var insurer = Insurer.Create(
            "12.345.678/0001-95", " Seguradora Alfa S.A. ", " Alfa ", "https://cdn.alfa.com/logo.png", EInsurerStatus.Active);

        insurer.Cnpj.Should().Be("12345678000195");
        insurer.CorporateName.Should().Be("Seguradora Alfa S.A.");
        insurer.TradeName.Should().Be("Alfa");
        insurer.LogoUrl.Should().Be("https://cdn.alfa.com/logo.png");
        insurer.Status.Should().Be(EInsurerStatus.Active);
    }

    [Fact]
    [Trait("RuleId", "RN-007")]
    public void Create_DeveAceitarOpcionaisAusentes_QuandoNomeFantasiaELogoNulos()
    {
        var insurer = NewInsurer();

        insurer.TradeName.Should().BeNull();
        insurer.LogoUrl.Should().BeNull();
    }

    [Fact]
    [Trait("RuleId", "RN-009")]
    public void Deactivate_DeveTornarInativa_QuandoAtiva()
    {
        var insurer = NewInsurer();

        insurer.Deactivate();

        insurer.Status.Should().Be(EInsurerStatus.Inactive);
    }

    [Fact]
    [Trait("RuleId", "RN-009")]
    public void Activate_DeveTornarAtiva_QuandoInativa()
    {
        var insurer = NewInsurer(EInsurerStatus.Inactive);

        insurer.Activate();

        insurer.Status.Should().Be(EInsurerStatus.Active);
    }

    [Fact]
    [Trait("RuleId", "RN-009")]
    public void Activate_DeveRecusar_QuandoJaAtiva()
    {
        var insurer = NewInsurer();

        var act = insurer.Activate;

        act.Should().Throw<ConflictException>();
    }

    [Fact]
    [Trait("RuleId", "RN-009")]
    public void Deactivate_DeveRecusar_QuandoJaInativa()
    {
        var insurer = NewInsurer(EInsurerStatus.Inactive);

        var act = insurer.Deactivate;

        act.Should().Throw<ConflictException>();
    }
}

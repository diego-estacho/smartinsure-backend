using FluentAssertions;
using SmartInsure.Application.UseCase.UseCases.InsurerUseCases.UpdateInsurer.Requests;
using SmartInsure.Application.UseCase.UseCases.InsurerUseCases.UpdateInsurer.Validators;

namespace SmartInsure.Tests.Application.UseCases.InsurerUseCases.UpdateInsurer;

/// <summary>RN-006 — validação de forma da alteração cadastral de Seguradora.</summary>
[Trait("RuleId", "RN-006")]
public class UpdateInsurerValidatorTests
{
    private readonly UpdateInsurerValidator _validator = new();

    private static UpdateInsurerRequest Request(
        Guid? insurerId = null,
        string cnpj = "12.345.678/0001-95",
        string corporateName = "Seguradora Alfa S.A.",
        string? tradeName = "Alfa",
        string? logoUrl = "https://cdn.alfa.com/logo.png")
        => new(insurerId ?? Guid.NewGuid(), cnpj, corporateName, tradeName, logoUrl);

    [Fact]
    public void Validate_DeveAprovar_QuandoRequestValido()
        => _validator.Validate(Request()).IsValid.Should().BeTrue();

    [Fact]
    public void Validate_DeveRecusar_QuandoCnpjComDigitosInvalidos()
        => _validator.Validate(Request(cnpj: "12345678000190")).IsValid.Should().BeFalse();

    [Fact]
    public void Validate_DeveRecusar_QuandoRazaoSocialAusente()
        => _validator.Validate(Request(corporateName: "")).IsValid.Should().BeFalse();

    [Fact]
    public void Validate_DeveRecusar_QuandoLogotipoNaoForUrlValida()
        => _validator.Validate(Request(logoUrl: "nao-e-url")).IsValid.Should().BeFalse();

    [Fact]
    public void Validate_DeveAprovar_QuandoOpcionaisAusentes()
        => _validator.Validate(Request(tradeName: null, logoUrl: null)).IsValid.Should().BeTrue();
}

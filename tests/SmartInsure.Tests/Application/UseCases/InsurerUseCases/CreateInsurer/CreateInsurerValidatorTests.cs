using FluentAssertions;
using SmartInsure.Application.UseCase.UseCases.InsurerUseCases.CreateInsurer.Requests;
using SmartInsure.Application.UseCase.UseCases.InsurerUseCases.CreateInsurer.Validators;

namespace SmartInsure.Tests.Application.UseCases.InsurerUseCases.CreateInsurer;

/// <summary>RN-007 — validação de forma do cadastro de Seguradora.</summary>
[Trait("RuleId", "RN-007")]
public class CreateInsurerValidatorTests
{
    private readonly CreateInsurerValidator _validator = new();

    private static CreateInsurerRequest Request(
        string cnpj = "12.345.678/0001-95",
        string corporateName = "Seguradora Alfa S.A.",
        string? tradeName = "Alfa",
        string? logoUrl = "https://cdn.alfa.com/logo.png",
        string initialStatus = "Active")
        => new(cnpj, corporateName, tradeName, logoUrl, initialStatus);

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
    public void Validate_DeveRecusar_QuandoSituacaoInicialAusenteOuDesconhecida()
        => _validator.Validate(Request(initialStatus: "Suspensa")).IsValid.Should().BeFalse();

    [Fact]
    public void Validate_DeveRecusar_QuandoLogotipoNaoForUrlValida()
        => _validator.Validate(Request(logoUrl: "nao-e-url")).IsValid.Should().BeFalse();

    [Fact]
    public void Validate_DeveAprovar_QuandoOpcionaisAusentes()
        => _validator.Validate(Request(tradeName: null, logoUrl: null)).IsValid.Should().BeTrue();
}

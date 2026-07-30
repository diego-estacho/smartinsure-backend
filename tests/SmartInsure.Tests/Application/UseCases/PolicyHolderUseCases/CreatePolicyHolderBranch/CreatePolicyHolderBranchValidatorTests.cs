using FluentAssertions;
using SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.CreatePolicyHolderBranch.Requests;
using SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.CreatePolicyHolderBranch.Validators;

namespace SmartInsure.Tests.Application.UseCases.PolicyHolderUseCases.CreatePolicyHolderBranch;

/// <summary>RN-101 — validação de forma do cadastro de Filial na ficha do Tomador.</summary>
[Trait("RuleId", "RN-101")]
public class CreatePolicyHolderBranchValidatorTests
{
    private readonly CreatePolicyHolderBranchValidator _validator = new();

    private static CreatePolicyHolderBranchRequest Request(
        Guid? policyHolderId = null,
        string documentNumber = "12345678000195")
        => new(policyHolderId ?? Guid.NewGuid(), documentNumber);

    [Fact]
    public void Validate_DeveAprovar_QuandoRequestValido()
        => _validator.Validate(Request()).IsValid.Should().BeTrue();

    [Fact]
    public void Validate_DeveRecusar_QuandoCnpjComDigitosInvalidos()
        => _validator.Validate(Request(documentNumber: "12345678000190")).IsValid.Should().BeFalse();

    [Fact]
    public void Validate_DeveRecusar_QuandoCnpjAusente()
        => _validator.Validate(Request(documentNumber: "")).IsValid.Should().BeFalse();

    [Fact]
    public void Validate_DeveRecusar_QuandoIdDoTomadorAusente()
        => _validator.Validate(Request(policyHolderId: Guid.Empty)).IsValid.Should().BeFalse();
}

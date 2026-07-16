using FluentAssertions;
using SmartInsure.Application.UseCase.UseCases.InsurerUseCases.ChangeInsurerStatus.Requests;
using SmartInsure.Application.UseCase.UseCases.InsurerUseCases.ChangeInsurerStatus.Validators;

namespace SmartInsure.Tests.Application.UseCases.InsurerUseCases.ChangeInsurerStatus;

/// <summary>RN-007 — validação de forma da alteração de situação de Seguradora.</summary>
[Trait("RuleId", "RN-007")]
public class ChangeInsurerStatusValidatorTests
{
    private readonly ChangeInsurerStatusValidator _validator = new();

    private static ChangeInsurerStatusRequest Request(
        Guid? insurerId = null,
        string status = "Active")
        => new(insurerId ?? Guid.NewGuid(), status);

    [Theory]
    [InlineData("Active")]
    [InlineData("Inactive")]
    public void Validate_DeveAprovar_QuandoStatusValido(string status)
        => _validator.Validate(Request(status: status)).IsValid.Should().BeTrue();

    [Fact]
    public void Validate_DeveRecusar_QuandoStatusVazio()
        => _validator.Validate(Request(status: "")).IsValid.Should().BeFalse();

    [Fact]
    public void Validate_DeveRecusar_QuandoStatusDesconhecido()
        => _validator.Validate(Request(status: "Suspensa")).IsValid.Should().BeFalse();
}

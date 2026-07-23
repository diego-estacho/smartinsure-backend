using FluentAssertions;
using SmartInsure.Application.UseCase.UseCases.CreditInquiryUseCases.ExecuteCreditInquiry.Requests;
using SmartInsure.Application.UseCase.UseCases.CreditInquiryUseCases.ExecuteCreditInquiry.Validators;

namespace SmartInsure.Tests.Application.UseCases.CreditInquiryUseCases.ExecuteCreditInquiry;

/// <summary>RN-029 — Validação de entrada da Consulta de Crédito (CNPJ, BrokerageId).</summary>
[Trait("RuleId", "RN-029")]
public class ExecuteCreditInquiryValidatorTests
{
    private static readonly Guid ValidBrokerageId = Guid.CreateVersion7();
    private static readonly string ValidCnpj = "12345678000195"; // Valid CNPJ with correct check digits

    private readonly ExecuteCreditInquiryValidator _validator = new();

    private static ExecuteCreditInquiryRequest ValidRequest()
        => new(ValidBrokerageId, ValidCnpj);

    [Fact]
    public void Validate_DeveAprovar_QuandoRequestValido()
    {
        var result = _validator.Validate(ValidRequest());

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_DeveRecusar_QuandoBrokerageIdVazio()
    {
        var request = new ExecuteCreditInquiryRequest(Guid.Empty, ValidCnpj);
        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "BrokerageId" &&
            e.ErrorMessage.Contains("obrigatório", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_DeveRecusar_QuandoCnpjVazio()
    {
        var request = new ExecuteCreditInquiryRequest(ValidBrokerageId, "");
        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "PolicyHolderCnpj" &&
            e.ErrorMessage.Contains("obrigatório", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_DeveRecusar_QuandoCnpjNulo()
    {
        var request = new ExecuteCreditInquiryRequest(ValidBrokerageId, null!);
        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "PolicyHolderCnpj" &&
            e.ErrorMessage.Contains("obrigatório", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_DeveRecusar_QuandoCnpjDigitosVerificadoresInvalidos()
    {
        // Valid CNPJ: 12345678000195
        // Invalid (wrong check digits): 12345678000190
        var request = new ExecuteCreditInquiryRequest(ValidBrokerageId, "12345678000190");
        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "PolicyHolderCnpj" &&
            e.ErrorMessage.Contains("inválido", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_DeveRecusar_QuandoCnpjComTamanhoErrado()
    {
        // CNPJ must have 14 digits
        var request = new ExecuteCreditInquiryRequest(ValidBrokerageId, "1234567800019"); // 13 digits
        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "PolicyHolderCnpj" &&
            e.ErrorMessage.Contains("inválido", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_DeveRecusar_QuandoCnpjComTodosMesmosDigitos()
    {
        // CNPJ with all same digits is always invalid per the CNPJ algorithm
        var request = new ExecuteCreditInquiryRequest(ValidBrokerageId, "11111111111111");
        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "PolicyHolderCnpj" &&
            e.ErrorMessage.Contains("inválido", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_DeveAprovar_QuandoCnpjFormatadoComPontuacao()
    {
        // CNPJ with formatting (dots and dashes) should be normalized and validated
        // 12.345.678/0001-95 normalizes to 12345678000195
        var request = new ExecuteCreditInquiryRequest(ValidBrokerageId, "12.345.678/0001-95");
        var result = _validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_DeveRetornarErrosEspecificos_QuandoMultiplosProblemasExistem()
    {
        var request = new ExecuteCreditInquiryRequest(Guid.Empty, "");
        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        // BrokerageId has 1 error, PolicyHolderCnpj has 2 errors (empty + invalid format)
        result.Errors.Should().HaveCountGreaterThanOrEqualTo(2);
        result.Errors.Should().Contain(e => e.PropertyName == "BrokerageId");
        result.Errors.Should().Contain(e => e.PropertyName == "PolicyHolderCnpj");
    }
}

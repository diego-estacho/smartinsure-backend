using FluentAssertions;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Entities;

namespace SmartInsure.Tests.Core.Entities;

/// <summary>
/// RN-505 / RN-510 — a Cotação registra as opções de pagamento e os documentos exigidos que a
/// Seguradora informou, para a etapa de emissão escolher sem acionar o provedor de novo.
/// </summary>
public class QuotationProviderOptionsTests
{
    private static Quotation NewQuotation()
        => Quotation.Requested(Guid.CreateVersion7(), Guid.CreateVersion7());

    [Fact]
    [Trait("RuleId", "RN-505")]
    public void SetProviderOptions_DeveGuardarEDevolverAsOpcoesDeParcelamento()
    {
        var quotation = NewQuotation();

        quotation.SetProviderOptions(
            [
                new QuotationInstallmentOption { Number = 1, Description = "À vista", Value = 300m, HasInterest = false },
                new QuotationInstallmentOption { Number = 3, Description = "3x com juros", Value = 105m, HasInterest = true },
            ],
            [0, 15, 30],
            []);

        var options = quotation.ReadInstallmentOptions();

        options.Should().HaveCount(2);
        options[0].Number.Should().Be(1);
        options[0].Value.Should().Be(300m);
        options[1].Number.Should().Be(3);
        options[1].HasInterest.Should().BeTrue();
        quotation.ReadPossibleGracePeriodsInDays().Should().Equal(0, 15, 30);
    }

    [Fact]
    [Trait("RuleId", "RN-510")]
    public void SetProviderOptions_DeveGuardarEDevolverOsDocumentosExigidos()
    {
        var quotation = NewQuotation();

        quotation.SetProviderOptions([], [], [new QuotationRequiredDocument { Name = "Contrato social" }]);

        quotation.ReadRequiredDocuments().Should().ContainSingle()
            .Which.Name.Should().Be("Contrato social");
    }

    [Fact]
    [Trait("RuleId", "RN-505")]
    public void ReadInstallmentOptions_SemOpcoesRegistradas_DeveDevolverVazio()
    {
        var quotation = NewQuotation();

        quotation.ReadInstallmentOptions().Should().BeEmpty();
        quotation.ReadPossibleGracePeriodsInDays().Should().BeEmpty();
        quotation.ReadRequiredDocuments().Should().BeEmpty();
    }

    [Fact]
    [Trait("RuleId", "RN-505")]
    public void HasInstallmentOption_DeveReconhecerSomenteAsOpcoesInformadasPelaSeguradora()
    {
        var quotation = NewQuotation();

        quotation.SetProviderOptions(
            [new QuotationInstallmentOption { Number = 1, Value = 300m }],
            [0, 30],
            []);

        quotation.HasInstallmentOption(1).Should().BeTrue();
        quotation.HasInstallmentOption(4).Should().BeFalse();
        quotation.HasGracePeriodOption(30).Should().BeTrue();
        quotation.HasGracePeriodOption(45).Should().BeFalse();
    }
}

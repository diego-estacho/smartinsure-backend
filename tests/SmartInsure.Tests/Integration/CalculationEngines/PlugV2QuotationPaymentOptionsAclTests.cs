using FluentAssertions;
using SmartInsure.Integration.CalculationEngines.PlugV2;

namespace SmartInsure.Tests.Integration.CalculationEngines;

/// <summary>
/// RN-505 / RN-510 — a Seguradora informa, na própria resposta da cotação, as opções de parcelamento,
/// os dias possíveis para vencimento da primeira parcela e os documentos exigidos. A ACL traduz esses
/// dados para o contrato do motor (ADR-045): é deles que a etapa de emissão tira a forma de pagamento,
/// sem chamada extra ao provedor.
/// </summary>
public class PlugV2QuotationPaymentOptionsAclTests
{
    private static PlugV2CotationData ReadyForEmission(
        IReadOnlyList<PlugV2InstallmentOption>? installmentOptions = null,
        IReadOnlyList<int>? gracePeriods = null,
        IReadOnlyList<PlugV2RequiredDocument>? documents = null)
        => new()
        {
            ResponseStatus = new PlugV2ResponseStatus { Status = 1 },
            Success = true,
            InsurancePremium = 300m,
            InstallmentOptions = installmentOptions,
            PossibleGracePeriodsInDays = gracePeriods,
            Documents = documents,
        };

    [Fact]
    [Trait("RuleId", "RN-505")]
    public void Map_DeveTraduzirAsOpcoesDeParcelamentoInformadasPelaSeguradora()
    {
        var response = ReadyForEmission(installmentOptions:
        [
            new PlugV2InstallmentOption { Number = 1, Description = "À vista", Value = 300m, HasInterest = false },
            new PlugV2InstallmentOption { Number = 3, Description = "3x com juros", Value = 105m, HasInterest = true },
        ]);

        var result = PlugV2QuotationAclMapper.Map(response);

        result.InstallmentOptions.Should().HaveCount(2);
        result.InstallmentOptions[0].Number.Should().Be(1);
        result.InstallmentOptions[0].Description.Should().Be("À vista");
        result.InstallmentOptions[0].Value.Should().Be(300m);
        result.InstallmentOptions[0].HasInterest.Should().BeFalse();
        result.InstallmentOptions[1].Number.Should().Be(3);
        result.InstallmentOptions[1].HasInterest.Should().BeTrue();
    }

    [Fact]
    [Trait("RuleId", "RN-505")]
    public void Map_DeveTraduzirOsDiasPossiveisDeVencimentoDaPrimeiraParcela()
    {
        var result = PlugV2QuotationAclMapper.Map(ReadyForEmission(gracePeriods: [0, 15, 30]));

        result.PossibleGracePeriodsInDays.Should().Equal(0, 15, 30);
    }

    [Fact]
    [Trait("RuleId", "RN-510")]
    public void Map_DeveTraduzirOsDocumentosExigidosPelaSeguradora()
    {
        var result = PlugV2QuotationAclMapper.Map(ReadyForEmission(documents:
        [
            new PlugV2RequiredDocument { Name = "Contrato social", Description = "Última alteração consolidada" },
        ]));

        result.RequiredDocuments.Should().HaveCount(1);
        result.RequiredDocuments[0].Name.Should().Be("Contrato social");
        result.RequiredDocuments[0].Description.Should().Be("Última alteração consolidada");
    }

    [Fact]
    [Trait("RuleId", "RN-505")]
    public void Map_SemOpcoesInformadas_NaoDeveInventarNada()
    {
        var result = PlugV2QuotationAclMapper.Map(ReadyForEmission());

        result.InstallmentOptions.Should().BeEmpty();
        result.PossibleGracePeriodsInDays.Should().BeEmpty();
        result.RequiredDocuments.Should().BeEmpty();
    }
}

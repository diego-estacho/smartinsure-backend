using FluentAssertions;
using SmartInsure.Core.Enumerators;
using SmartInsure.Integration.CalculationEngines.PlugV2;

namespace SmartInsure.Tests.Integration.CalculationEngines;

/// <summary>
/// RN-058 / ADR-064 — ACL do resultado da Cotação PlugV2: de-para dos 11 status do eixo imediato +
/// desconhecido → Unrecognized (nunca silêncio) + captura de CCG. Antídoto da divergência do legado.
/// </summary>
[Trait("RuleId", "RN-058")]
public class PlugV2QuotationAclMapperTests
{
    private static PlugV2CotationData Response(
        int status,
        decimal? premium = null,
        List<string>? erros = null,
        PlugV2CcgResult? ccg = null,
        string? proposalNumber = null,
        string? proposalUniqueId = null)
        => new()
        {
            ResponseStatus = new PlugV2ResponseStatus { Status = status },
            Success = status == 1,
            InsurancePremium = premium,
            Erros = erros,
            PolicyHolderCcg = ccg,
            ProposalNumber = proposalNumber,
            ProposalUniqueId = proposalUniqueId,
        };

    [Fact]
    public void Map_DeveClassificarSuccessComoAutomaticComPremio()
    {
        var result = PlugV2QuotationAclMapper.Map(Response(1, premium: 300m));

        result.Result.Should().Be(EQuotationResult.ReadyForEmission);
        result.Premium.Should().Be(300m);
    }

    [Theory]
    [InlineData(5, EAnalysisTrack.Underwriting)]
    [InlineData(2, EAnalysisTrack.Registration)]
    [InlineData(3, EAnalysisTrack.Pep)]
    [InlineData(4, EAnalysisTrack.Credit)]
    [InlineData(6, EAnalysisTrack.Reinsurance)]
    public void Map_DeveClassificarKanbanComoAnalysisComEsteira(int status, EAnalysisTrack expectedTrack)
    {
        var result = PlugV2QuotationAclMapper.Map(Response(status));

        result.Result.Should().Be(EQuotationResult.Analysis);
        result.AnalysisTrack.Should().Be(expectedTrack);
        result.Premium.Should().BeNull();
    }

    [Theory]
    [InlineData(7)]  // Modalidade indisponível
    [InlineData(9)]  // Cobertura indisponível
    [InlineData(8)]  // Tomador nomeado
    [InlineData(99)] // Erro técnico
    public void Map_DeveClassificarIndisponibilidadesComoUnavailableComMotivo(int status)
    {
        var result = PlugV2QuotationAclMapper.Map(Response(status));

        result.Result.Should().Be(EQuotationResult.Unavailable);
        result.Reasons.Should().NotBeEmpty();
        result.Premium.Should().BeNull();
    }

    [Fact]
    public void Map_DeveUsarErrosDoProvedorComoMotivos_QuandoPresentes()
    {
        var result = PlugV2QuotationAclMapper.Map(Response(7, erros: ["Motivo específico da Seguradora"]));

        result.Reasons.Should().ContainSingle().Which.Should().Be("Motivo específico da Seguradora");
    }

    [Fact]
    public void Map_DeveClassificarComoUnavailableComMotivo_QuandoEnvelopeSinalizaHasError_MesmoComStatusSuccess()
    {
        // HasError no envelope: não confia no status/prêmio do payload — Indisponível com o motivo do gateway.
        var result = PlugV2QuotationAclMapper.Map(
            Response(1, premium: 300m), hasError: true, envelopeErrors: ["Erro no motor da Seguradora"]);

        result.Result.Should().Be(EQuotationResult.Unavailable);
        result.Premium.Should().BeNull();
        result.Reasons.Should().Contain("Erro no motor da Seguradora");
    }

    [Fact]
    public void Map_DeveClassificarUnknowComoUnrecognized()
    {
        var result = PlugV2QuotationAclMapper.Map(Response(0));

        result.Result.Should().Be(EQuotationResult.Unrecognized);
        result.Premium.Should().BeNull();
        result.AnalysisTrack.Should().BeNull();
    }

    [Fact]
    public void Map_DeveClassificarStatusDesconhecidoComoUnrecognized_NuncaSilencio()
    {
        // Status fora dos 11 conhecidos (ex.: novo no gateway) — jamais vira Automatic nem exibe prêmio.
        var result = PlugV2QuotationAclMapper.Map(Response(4242, premium: 999m));

        result.Result.Should().Be(EQuotationResult.Unrecognized);
        result.Premium.Should().BeNull();
        result.AnalysisTrack.Should().BeNull();
    }

    [Fact]
    public void Map_DeveIgnorarPremio_QuandoNaoSuccess()
    {
        // Prêmio só é lido em Automatic (RN-058): mesmo se o provedor mandar prêmio numa Análise, não expõe.
        var result = PlugV2QuotationAclMapper.Map(Response(5, premium: 500m));

        result.Result.Should().Be(EQuotationResult.Analysis);
        result.Premium.Should().BeNull();
    }

    [Fact]
    public void Map_DeveCapturarNumeroEIdDaProposta_EmAnalysis()
    {
        // O provedor emite proposta (número/ID) mesmo em esteira de análise — capturamos para a Cotação
        // ter rastreio na Listagem/acompanhamento. Diferente do prêmio, que só sai no seguível.
        var result = PlugV2QuotationAclMapper.Map(
            Response(5, proposalNumber: "202600000274221", proposalUniqueId: "abc-123"));

        result.Result.Should().Be(EQuotationResult.Analysis);
        result.ProposalNumber.Should().Be("202600000274221");
        result.ProposalExternalId.Should().Be("abc-123");
        result.Premium.Should().BeNull();
    }

    [Fact]
    public void Map_DeveCapturarCcg_IndependenteDaClassificacao()
    {
        var ccg = new PlugV2CcgResult { RequiresCcg = true, MaxLimitWithoutNeedCcg = 500_000m, HasSignedCcg = false };

        var result = PlugV2QuotationAclMapper.Map(Response(1, premium: 300m, ccg: ccg));

        result.RequiresCcg.Should().BeTrue();
        result.CcgMaxLimitWithoutNeed.Should().Be(500_000m);
        result.CcgSigned.Should().BeFalse();
    }
}

using FluentAssertions;
using SmartInsure.Core.Enumerators;
using SmartInsure.Integration.CalculationEngines.PlugV2;

namespace SmartInsure.Tests.Integration.CalculationEngines;

/// <summary>RN-058 / ADR-064 — ACL do resultado de cotação PlugV2: de-para dos 11 status + CCG + Unrecognized.</summary>
[Trait("RuleId", "RN-058")]
public class PlugV2QuotationAclMapperTests
{
    [Fact]
    public void Map_DeveClassificarAutomatico_ComPremioECcg_QuandoSuccess()
    {
        const string raw = """
        {"statusCode":200,"hasError":false,"response":{"status":"SUCCESS",
         "insurancePremium":1500.50,"comissionPercentage":10,"comissionValue":150,"tax":0.5,
         "policyHolderAvailableLimit":100000,"proposalUniqueId":"P-1","proposalNumber":"123",
         "ccg":{"requiresCCG":false}}}
        """;

        var result = PlugV2QuotationAclMapper.Map(raw);

        result.Result.Should().Be(EQuotationResult.Automatic);
        result.Premium.Should().Be(1500.50m);
        result.ProposalExternalId.Should().Be("P-1");
        result.RequiresCcg.Should().BeFalse();
    }

    [Fact]
    public void Map_DeveClassificarAnaliseSubscricao_SemPremio_ComCcg_QuandoKanbanSubscricao()
    {
        const string raw = """
        {"statusCode":200,"hasError":false,"response":{"status":"KANBAN_SUBSCRICAO",
         "insurancePremium":999,"ccg":{"requiresCCG":true,"maxLimitWithoutNeedCCG":50000,"hasSignedCCG":false}}}
        """;

        var result = PlugV2QuotationAclMapper.Map(raw);

        result.Result.Should().Be(EQuotationResult.Analysis);
        result.AnalysisTrack.Should().Be(EAnalysisTrack.Underwriting);
        result.Premium.Should().BeNull();
        result.RequiresCcg.Should().BeTrue();
        result.CcgMaxLimitWithoutNeed.Should().Be(50000m);
    }

    [Theory]
    [InlineData("KANBAN_CADASTRO", "Registration")]
    [InlineData("KANBAN_PEP", "Pep")]
    [InlineData("KANBAN_CREDITO", "Credit")]
    [InlineData("KANBAN_RESSEGURO", "Reinsurance")]
    public void Map_DeveMapearEsteirasEspecificas(string status, string expectedTrack)
    {
        var raw = "{\"statusCode\":200,\"hasError\":false,\"response\":{\"status\":\"" + status + "\"}}";

        var result = PlugV2QuotationAclMapper.Map(raw);

        result.Result.Should().Be(EQuotationResult.Analysis);
        result.AnalysisTrack!.ToString().Should().Be(expectedTrack);
    }

    [Fact]
    public void Map_DeveDerivarMotivo_QuandoIndisponivelSemErros()
    {
        const string raw = """{"statusCode":200,"hasError":false,"response":{"status":"MODALIDADE_INDISPONIVEL","erros":[]}}""";

        var result = PlugV2QuotationAclMapper.Map(raw);

        result.Result.Should().Be(EQuotationResult.Unavailable);
        result.Reasons.Should().ContainSingle();
    }

    [Fact]
    public void Map_DevePreservarMotivosDaSeguradora_QuandoErro()
    {
        const string raw = """
        {"statusCode":200,"hasError":false,"response":{"status":"ERROR","erros":["Limite insuficiente","Restrição cadastral"]}}
        """;

        var result = PlugV2QuotationAclMapper.Map(raw);

        result.Result.Should().Be(EQuotationResult.Unavailable);
        result.Reasons.Should().HaveCount(2);
    }

    [Fact]
    public void Map_DeveRecairEmUnrecognized_QuandoStatusDesconhecido()
    {
        const string raw = """{"statusCode":200,"hasError":false,"response":{"status":"KANBAN_ALGO_NOVO","insurancePremium":10}}""";

        var result = PlugV2QuotationAclMapper.Map(raw);

        result.Result.Should().Be(EQuotationResult.Unrecognized);
        result.Premium.Should().BeNull();
    }

    [Fact]
    public void Map_DeveRecairEmUnrecognized_QuandoJsonInvalido()
    {
        var result = PlugV2QuotationAclMapper.Map("isto não é json");

        result.Result.Should().Be(EQuotationResult.Unrecognized);
    }

    [Fact]
    public void Map_DeveRecairEmUnrecognized_QuandoSemResponse()
    {
        var result = PlugV2QuotationAclMapper.Map("""{"statusCode":500,"hasError":true,"response":null}""");

        result.Result.Should().Be(EQuotationResult.Unrecognized);
    }
}

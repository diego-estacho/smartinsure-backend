using FluentAssertions;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;

namespace SmartInsure.Tests.Core.Entities;

/// <summary>RN-057/RN-058/RN-059 — Entidade Cotação: ciclo Requested→Obtida/Falha, prêmio e seguibilidade.</summary>
[Trait("RuleId", "RN-057")]
[Trait("RuleId", "RN-058")]
[Trait("RuleId", "RN-059")]
public class QuotationTests
{
    private static readonly Guid GroupId = Guid.CreateVersion7();
    private static readonly Guid BrokerageId = Guid.CreateVersion7();
    private static readonly Guid InsurerId = Guid.CreateVersion7();

    private static Quotation NewRequested()
        => Quotation.Request(GroupId, BrokerageId, InsurerId);

    [Fact]
    public void Request_DeveNascerRequested()
    {
        var quotation = NewRequested();

        quotation.ProcessingStatus.Should().Be(EQuotationProcessingStatus.Requested);
        quotation.Result.Should().BeNull();
        quotation.IsFollowable.Should().BeFalse();
    }

    [Fact]
    public void MarkObtained_Automatica_DeveManterPremio()
    {
        var quotation = NewRequested();

        quotation.MarkObtained(
            EQuotationResult.Automatic, null, 1500m, 10m, 150m, 0.5m, 100000m,
            "P-1", "123", requiresCcg: false, ccgMaxLimitWithoutNeed: null, ccgSigned: false, reasons: []);

        quotation.ProcessingStatus.Should().Be(EQuotationProcessingStatus.Obtained);
        quotation.Result.Should().Be(EQuotationResult.Automatic);
        quotation.Premium.Should().Be(1500m);
        quotation.IsFollowable.Should().BeTrue();
    }

    [Fact]
    public void MarkObtained_Analise_DeveDescartarPremio_EGuardarEsteiraEMotivos()
    {
        var quotation = NewRequested();

        quotation.MarkObtained(
            EQuotationResult.Analysis, EAnalysisTrack.Credit, 999m, 10m, 99m, 0.5m, null,
            null, null, requiresCcg: false, ccgMaxLimitWithoutNeed: null, ccgSigned: false,
            reasons: ["Aguardando análise de crédito"]);

        quotation.Premium.Should().BeNull();
        quotation.AnalysisTrack.Should().Be(EAnalysisTrack.Credit);
        quotation.Reasons.Should().ContainSingle();
    }

    [Fact]
    public void MarkFailed_DeveVirarUnavailable_ComMotivo()
    {
        var quotation = NewRequested();

        quotation.MarkFailed("Tempo-limite excedido");

        quotation.ProcessingStatus.Should().Be(EQuotationProcessingStatus.Failed);
        quotation.Result.Should().Be(EQuotationResult.Unavailable);
        quotation.Reasons.Should().ContainSingle();
        quotation.IsFollowable.Should().BeFalse();
    }

    [Fact]
    public void IsFollowable_Automatica_DeveSerTrue()
    {
        var quotation = NewRequested();
        quotation.MarkObtained(EQuotationResult.Automatic, null, 1m, null, null, null, null, null, null, false, null, false, []);

        quotation.IsFollowable.Should().BeTrue();
    }

    [Fact]
    public void IsFollowable_AnaliseSubscricao_DeveSerTrue()
    {
        var quotation = NewRequested();
        quotation.MarkObtained(EQuotationResult.Analysis, EAnalysisTrack.Underwriting, null, null, null, null, null, null, null, false, null, false, []);

        quotation.IsFollowable.Should().BeTrue();
    }

    [Theory]
    [InlineData(EAnalysisTrack.Credit)]
    [InlineData(EAnalysisTrack.Pep)]
    [InlineData(EAnalysisTrack.Reinsurance)]
    [InlineData(EAnalysisTrack.Registration)]
    public void IsFollowable_AnaliseOutrasEsteiras_DeveSerFalse(EAnalysisTrack track)
    {
        var quotation = NewRequested();
        quotation.MarkObtained(EQuotationResult.Analysis, track, null, null, null, null, null, null, null, false, null, false, []);

        quotation.IsFollowable.Should().BeFalse();
    }

    [Fact]
    public void IsFollowable_Unrecognized_DeveSerFalse()
    {
        var quotation = NewRequested();
        quotation.MarkObtained(EQuotationResult.Unrecognized, null, null, null, null, null, null, null, null, false, null, false, []);

        quotation.IsFollowable.Should().BeFalse();
    }
}

using FluentAssertions;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;

namespace SmartInsure.Tests.Core.Entities;

/// <summary>
/// RN-058/RN-059 — Entidade Quotation: classificação estável + invariantes (ADR-064) e seguibilidade.
/// </summary>
[Trait("RuleId", "RN-058")]
[Trait("RuleId", "RN-059")]
public class QuotationTests
{
    private static readonly Guid GroupId = Guid.CreateVersion7();
    private static readonly Guid InsurerId = Guid.CreateVersion7();
    private static readonly DateTime ObtainedAt = new(2026, 7, 28, 12, 0, 0, DateTimeKind.Utc);

    private static Quotation Requested() => Quotation.Requested(GroupId, InsurerId);

    [Fact]
    public void Requested_DeveNascerRequested_SemResultado()
    {
        var quotation = Requested();

        quotation.QuotationGroupId.Should().Be(GroupId);
        quotation.InsurerId.Should().Be(InsurerId);
        quotation.ProcessingStatus.Should().Be(EQuotationProcessingStatus.Requested);
        quotation.Result.Should().BeNull();
        quotation.IsFollowable.Should().BeFalse();
    }

    [Fact]
    public void MarkObtained_DeveGravarAutomaticComPremio_ESeguivel()
    {
        var quotation = Requested();

        quotation.MarkObtained(
            EQuotationResult.ReadyForEmission, analysisTrack: null,
            premium: 300m, commissionPercentage: 25m, commissionValue: 75m, tax: 0.42m, availableLimit: 1_000_000m,
            proposalExternalId: "abc", proposalNumber: "P-1",
            requiresCcg: false, ccgMaxLimitWithoutNeed: null, ccgSigned: false,
            reasonTexts: [], obtainedAt: ObtainedAt);

        quotation.ProcessingStatus.Should().Be(EQuotationProcessingStatus.Obtained);
        quotation.Result.Should().Be(EQuotationResult.ReadyForEmission);
        quotation.Premium.Should().Be(300m);
        quotation.ObtainedAt.Should().Be(ObtainedAt);
        quotation.IsFollowable.Should().BeTrue();
    }

    [Fact]
    public void MarkObtained_DeveExigirEsteira_QuandoAnalysis()
    {
        var quotation = Requested();

        var act = () => quotation.MarkObtained(
            EQuotationResult.Analysis, analysisTrack: null,
            premium: null, commissionPercentage: null, commissionValue: null, tax: null, availableLimit: null,
            proposalExternalId: null, proposalNumber: null,
            requiresCcg: false, ccgMaxLimitWithoutNeed: null, ccgSigned: false,
            reasonTexts: [], obtainedAt: ObtainedAt);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void MarkObtained_DeveRecusarPremio_QuandoNaoAutomatic()
    {
        var quotation = Requested();

        var act = () => quotation.MarkObtained(
            EQuotationResult.Analysis, analysisTrack: EAnalysisTrack.Underwriting,
            premium: 300m, commissionPercentage: null, commissionValue: null, tax: null, availableLimit: null,
            proposalExternalId: null, proposalNumber: null,
            requiresCcg: false, ccgMaxLimitWithoutNeed: null, ccgSigned: false,
            reasonTexts: [], obtainedAt: ObtainedAt);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void MarkObtained_DeveExigirMotivo_QuandoUnavailable()
    {
        var quotation = Requested();

        var act = () => quotation.MarkObtained(
            EQuotationResult.Unavailable, analysisTrack: null,
            premium: null, commissionPercentage: null, commissionValue: null, tax: null, availableLimit: null,
            proposalExternalId: null, proposalNumber: null,
            requiresCcg: false, ccgMaxLimitWithoutNeed: null, ccgSigned: false,
            reasonTexts: [], obtainedAt: ObtainedAt);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void MarkObtained_DeveGravarUnavailableComMotivosDoProvedor()
    {
        var quotation = Requested();

        quotation.MarkObtained(
            EQuotationResult.Unavailable, analysisTrack: null,
            premium: null, commissionPercentage: null, commissionValue: null, tax: null, availableLimit: null,
            proposalExternalId: null, proposalNumber: null,
            requiresCcg: false, ccgMaxLimitWithoutNeed: null, ccgSigned: false,
            reasonTexts: ["Modalidade indisponível"], obtainedAt: ObtainedAt);

        quotation.Result.Should().Be(EQuotationResult.Unavailable);
        quotation.Premium.Should().BeNull();
        quotation.Reasons.Should().ContainSingle();
        quotation.Reasons.First().Source.Should().Be(EQuotationReasonSource.Provider);
        quotation.IsFollowable.Should().BeFalse();
    }

    [Fact]
    public void MarkObtained_DeveGravarUnrecognizedSemPremioSemEsteira_NaoSeguivel()
    {
        var quotation = Requested();

        quotation.MarkObtained(
            EQuotationResult.Unrecognized, analysisTrack: null,
            premium: null, commissionPercentage: null, commissionValue: null, tax: null, availableLimit: null,
            proposalExternalId: null, proposalNumber: null,
            requiresCcg: false, ccgMaxLimitWithoutNeed: null, ccgSigned: false,
            reasonTexts: [], obtainedAt: ObtainedAt);

        quotation.Result.Should().Be(EQuotationResult.Unrecognized);
        quotation.Premium.Should().BeNull();
        quotation.AnalysisTrack.Should().BeNull();
        quotation.IsFollowable.Should().BeFalse();
    }

    [Theory]
    [InlineData(EAnalysisTrack.Underwriting, true)]
    [InlineData(EAnalysisTrack.Credit, false)]
    [InlineData(EAnalysisTrack.Pep, false)]
    [InlineData(EAnalysisTrack.Reinsurance, false)]
    [InlineData(EAnalysisTrack.Registration, false)]
    public void IsFollowable_DependeDaEsteira_QuandoAnalysis(EAnalysisTrack track, bool expected)
    {
        var quotation = Requested();

        quotation.MarkObtained(
            EQuotationResult.Analysis, analysisTrack: track,
            premium: null, commissionPercentage: null, commissionValue: null, tax: null, availableLimit: null,
            proposalExternalId: null, proposalNumber: null,
            requiresCcg: false, ccgMaxLimitWithoutNeed: null, ccgSigned: false,
            reasonTexts: [], obtainedAt: ObtainedAt);

        quotation.IsFollowable.Should().Be(expected);
    }

    [Fact]
    public void IsFollowable_CcgNaoBloqueia_QuandoAutomatic()
    {
        var quotation = Requested();

        quotation.MarkObtained(
            EQuotationResult.ReadyForEmission, analysisTrack: null,
            premium: 300m, commissionPercentage: null, commissionValue: null, tax: null, availableLimit: null,
            proposalExternalId: null, proposalNumber: null,
            requiresCcg: true, ccgMaxLimitWithoutNeed: 500_000m, ccgSigned: false,
            reasonTexts: [], obtainedAt: ObtainedAt);

        quotation.RequiresCcg.Should().BeTrue();
        quotation.IsFollowable.Should().BeTrue();
    }

    [Fact]
    public void UnavailableLocal_DeveCriarIndisponivelComMotivoLocal()
    {
        var quotation = Quotation.UnavailableLocal(GroupId, InsurerId, "Não incluída na solicitação");

        quotation.Result.Should().Be(EQuotationResult.Unavailable);
        quotation.ProcessingStatus.Should().Be(EQuotationProcessingStatus.Obtained);
        quotation.Reasons.Should().ContainSingle();
        quotation.Reasons.First().Source.Should().Be(EQuotationReasonSource.Local);
        quotation.IsFollowable.Should().BeFalse();
    }

    [Fact]
    public void MarkFailed_DeveVirarIndisponivelComMotivoTecnicoLocal()
    {
        var quotation = Requested();

        quotation.MarkFailed("Timeout ao obter a Cotação.", ObtainedAt);

        quotation.ProcessingStatus.Should().Be(EQuotationProcessingStatus.Failed);
        quotation.Result.Should().Be(EQuotationResult.Unavailable);
        quotation.Reasons.Should().ContainSingle();
        quotation.Reasons.First().Source.Should().Be(EQuotationReasonSource.Local);
        quotation.IsFollowable.Should().BeFalse();
    }
}

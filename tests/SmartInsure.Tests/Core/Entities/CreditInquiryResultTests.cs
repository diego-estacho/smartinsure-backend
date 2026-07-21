using FluentAssertions;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;

namespace SmartInsure.Tests.Core.Entities;

/// <summary>RN-030/RN-031 — Entidade CreditInquiryResult: factories (Available/Unavailable), imutabilidade.</summary>
[Trait("RuleId", "RN-030")]
[Trait("RuleId", "RN-031")]
public class CreditInquiryResultTests
{
    private static readonly Guid CreditInquiryId = Guid.CreateVersion7();
    private static readonly Guid InsurerId = Guid.CreateVersion7();

    [Fact]
    public void Available_DeveCriarResultadoComStatus_QuandoChamaFactory()
    {
        var validUntil = DateTime.UtcNow.AddMonths(12);
        var result = CreditInquiryResult.Available(
            CreditInquiryId, InsurerId,
            1000m, 0.05m, 2000m, 0.06m, 0.07m, 3000m, 0.08m,
            validUntil);

        result.CreditInquiryId.Should().Be(CreditInquiryId);
        result.InsurerId.Should().Be(InsurerId);
        result.Status.Should().Be(ECreditInquiryResultStatus.Available);
        result.FailureReason.Should().BeNull();
        result.TraditionalLimit.Should().Be(1000m);
        result.TraditionalRate.Should().Be(0.05m);
        result.JudicialLimit.Should().Be(2000m);
        result.JudicialRate.Should().Be(0.06m);
        result.JudicialFiscalRate.Should().Be(0.07m);
        result.FinancialLimit.Should().Be(3000m);
        result.FinancialRate.Should().Be(0.08m);
        result.LimitValidUntil.Should().Be(validUntil);
    }

    [Fact]
    public void Available_DevePermitirModalidadesOpcionais_QuandoNullsPassados()
    {
        var result = CreditInquiryResult.Available(
            CreditInquiryId, InsurerId,
            1000m, 0.05m,
            null, null, null, // Judicial (optional)
            null, null, // Financial (optional)
            DateTime.UtcNow.AddMonths(6));

        result.Status.Should().Be(ECreditInquiryResultStatus.Available);
        result.TraditionalLimit.Should().Be(1000m);
        result.JudicialLimit.Should().BeNull();
        result.FinancialLimit.Should().BeNull();
    }

    [Fact]
    public void Available_DeveGerarIdUnico_QuandoMultiplosResultadosCriados()
    {
        var result1 = CreditInquiryResult.Available(
            CreditInquiryId, InsurerId,
            1000m, 0.05m, null, null, null, null, null, null);

        var result2 = CreditInquiryResult.Available(
            CreditInquiryId, InsurerId,
            1000m, 0.05m, null, null, null, null, null, null);

        result1.Id.Should().NotBe(result2.Id);
    }

    [Fact]
    public void Unavailable_DeveCriarResultadoComMotivo_QuandoChamaFactory()
    {
        var failureReason = "Seguradora indisponível";
        var result = CreditInquiryResult.Unavailable(CreditInquiryId, InsurerId, failureReason);

        result.CreditInquiryId.Should().Be(CreditInquiryId);
        result.InsurerId.Should().Be(InsurerId);
        result.Status.Should().Be(ECreditInquiryResultStatus.Unavailable);
        result.FailureReason.Should().Be(failureReason);
        result.TraditionalLimit.Should().BeNull();
        result.TraditionalRate.Should().BeNull();
        result.JudicialLimit.Should().BeNull();
        result.FinancialLimit.Should().BeNull();
        result.LimitValidUntil.Should().BeNull();
    }

}

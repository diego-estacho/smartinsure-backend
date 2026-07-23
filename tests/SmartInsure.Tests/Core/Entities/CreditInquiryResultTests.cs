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
        var resultId = Guid.CreateVersion7();
        var result = CreditInquiryResult.Available(
            CreditInquiryId, InsurerId,
            new[]
            {
                CreditInquiryResultLimit.Create("Tradicional", "GARANTIA_TRADICIONAL", 1000m, 1000m, 0.05m),
                CreditInquiryResultLimit.Create("Judicial", "GARANTIA_JUDICIAL", 2000m, 2000m, 0.06m),
                CreditInquiryResultLimit.Create("JudicialFiscal", "GARANTIA_JUDICIAL_FISCAL", 2000m, 2000m, 0.07m),
                CreditInquiryResultLimit.Create("Financial", "GARANTIA_FINANCEIRA", 3000m, 3000m, 0.08m)
            });

        result.CreditInquiryId.Should().Be(CreditInquiryId);
        result.InsurerId.Should().Be(InsurerId);
        result.Status.Should().Be(ECreditInquiryResultStatus.Available);
        result.FailureReason.Should().BeNull();
        result.Limits.First(l => l.GroupName == "Tradicional").AvailableLimit.Should().Be(1000m);
        result.Limits.First(l => l.GroupName == "Tradicional").Rate.Should().Be(0.05m);
        result.Limits.First(l => l.GroupName == "Judicial").AvailableLimit.Should().Be(2000m);
        result.Limits.First(l => l.GroupName == "Judicial").Rate.Should().Be(0.06m);
        result.Limits.First(l => l.GroupName == "JudicialFiscal").Rate.Should().Be(0.07m);
        result.Limits.First(l => l.GroupName == "Financial").AvailableLimit.Should().Be(3000m);
        result.Limits.First(l => l.GroupName == "Financial").Rate.Should().Be(0.08m);
    }

    [Fact]
    public void Available_DevePermitirModalidadesOpcionais_QuandoNullsPassados()
    {
        var resultId = Guid.CreateVersion7();
        var result = CreditInquiryResult.Available(
            CreditInquiryId, InsurerId,
            new[]
            {
                CreditInquiryResultLimit.Create("Tradicional", "GARANTIA_TRADICIONAL", 1000m, 1000m, 0.05m)
            });

        result.Status.Should().Be(ECreditInquiryResultStatus.Available);
        result.Limits.First(l => l.GroupName == "Tradicional").AvailableLimit.Should().Be(1000m);
        result.Limits.Any(l => l.GroupName == "Judicial").Should().BeFalse();
        result.Limits.Any(l => l.GroupName == "Financial").Should().BeFalse();
    }

    [Fact]
    public void Available_DeveGerarIdUnico_QuandoMultiplosResultadosCriados()
    {
        var resultId1 = Guid.CreateVersion7();
        var result1 = CreditInquiryResult.Available(
            CreditInquiryId, InsurerId,
            new[]
            {
                CreditInquiryResultLimit.Create("Tradicional", "GARANTIA_TRADICIONAL", 1000m, 1000m, 0.05m)
            });

        var resultId2 = Guid.CreateVersion7();
        var result2 = CreditInquiryResult.Available(
            CreditInquiryId, InsurerId,
            new[]
            {
                CreditInquiryResultLimit.Create("Tradicional", "GARANTIA_TRADICIONAL", 1000m, 1000m, 0.05m)
            });

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
        result.Limits.Should().BeEmpty();
    }

}

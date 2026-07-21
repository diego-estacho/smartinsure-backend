using FluentAssertions;
using SmartInsure.Core.Entities;

namespace SmartInsure.Tests.Core.Entities;

/// <summary>RN-031 — Entidade CreditInquiry: imutabilidade, factory, coleção de resultados.</summary>
[Trait("RuleId", "RN-031")]
public class CreditInquiryTests
{
    private static readonly Guid BrokerageId = Guid.CreateVersion7();
    private static readonly string PolicyHolderCnpj = "12345678000195";

    [Fact]
    public void Create_DeveGerarConsultaComDataAtual_QuandoChamaFactory()
    {
        var beforeCreation = DateTime.UtcNow;
        var inquiry = CreditInquiry.Create(BrokerageId, PolicyHolderCnpj);
        var afterCreation = DateTime.UtcNow;

        inquiry.BrokerageId.Should().Be(BrokerageId);
        inquiry.PolicyHolderCnpj.Should().Be(PolicyHolderCnpj);
        inquiry.QueriedAt.Should().BeOnOrAfter(beforeCreation);
        inquiry.QueriedAt.Should().BeOnOrBefore(afterCreation);
    }

    [Fact]
    public void Create_DeveGerarIdUnico_QuandoMultiplasConsultasCriadas()
    {
        var inquiry1 = CreditInquiry.Create(BrokerageId, PolicyHolderCnpj);
        var inquiry2 = CreditInquiry.Create(BrokerageId, PolicyHolderCnpj);

        inquiry1.Id.Should().NotBe(inquiry2.Id);
        inquiry1.Id.Should().NotBe(Guid.Empty);
        inquiry2.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Results_DeveRetornarColecaoImutavel_QuandoAcessado()
    {
        var inquiry = CreditInquiry.Create(BrokerageId, PolicyHolderCnpj);
        var results = inquiry.Results;

        results.Should().BeEmpty();
        results.Should().BeAssignableTo<IReadOnlyCollection<CreditInquiryResult>>();
    }

    [Fact]
    public void AddResult_DeveAdicionarResultadoValido_QuandoResultadoPertenceAConsulta()
    {
        var inquiry = CreditInquiry.Create(BrokerageId, PolicyHolderCnpj);
        var insurer1Id = Guid.CreateVersion7();

        var result = CreditInquiryResult.Available(
            inquiry.Id, insurer1Id,
            1000m, 0.05m, 2000m, 0.06m, 0.07m, 3000m, 0.08m,
            DateTime.UtcNow.AddMonths(12));

        inquiry.AddResult(result);

        inquiry.Results.Should().HaveCount(1);
        inquiry.Results.First().InsurerId.Should().Be(insurer1Id);
    }

    [Fact]
    public void AddResult_DeveAdicionarMultiplosResultados_QuandoTodasPertencemAConsulta()
    {
        var inquiry = CreditInquiry.Create(BrokerageId, PolicyHolderCnpj);
        var insurer1Id = Guid.CreateVersion7();
        var insurer2Id = Guid.CreateVersion7();

        var result1 = CreditInquiryResult.Available(
            inquiry.Id, insurer1Id,
            1000m, 0.05m, null, null, null, null, null,
            DateTime.UtcNow.AddMonths(12));

        var result2 = CreditInquiryResult.Unavailable(
            inquiry.Id, insurer2Id,
            "Sistema indisponível");

        inquiry.AddResult(result1);
        inquiry.AddResult(result2);

        inquiry.Results.Should().HaveCount(2);
        inquiry.Results.Should().Contain(r => r.InsurerId == insurer1Id && r.Status.ToString() == "Available");
        inquiry.Results.Should().Contain(r => r.InsurerId == insurer2Id && r.Status.ToString() == "Unavailable");
    }

    [Fact]
    public void AddResult_DeveLancarExcecao_QuandoResultadoNaoPertenceAConsulta()
    {
        var inquiry = CreditInquiry.Create(BrokerageId, PolicyHolderCnpj);
        var outherInquiryId = Guid.CreateVersion7();
        var insurerId = Guid.CreateVersion7();

        var resultWithWrongInquiryId = CreditInquiryResult.Available(
            outherInquiryId, insurerId,
            1000m, 0.05m, null, null, null, null, null,
            DateTime.UtcNow.AddMonths(12));

        var act = () => inquiry.AddResult(resultWithWrongInquiryId);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*resultado não pertence a esta consulta*");
    }

}

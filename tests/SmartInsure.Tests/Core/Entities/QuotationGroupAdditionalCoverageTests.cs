using FluentAssertions;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;

namespace SmartInsure.Tests.Core.Entities;

/// <summary>RN-104 — Coberturas Adicionais escolhidas no Grupo de Cotação (conjunto, pela canônica).</summary>
[Trait("RuleId", "RN-104")]
public sealed class QuotationGroupAdditionalCoverageTests
{
    [Fact]
    public void Create_DeveGuardarCoberturasSemRepeticao_RN104()
    {
        var multa = Guid.CreateVersion7();
        var trabalhista = Guid.CreateVersion7();

        var group = NewGroup([multa, trabalhista, multa]);

        group.AdditionalCoverages.Select(coverage => coverage.AdditionalCoverageId)
            .Should().BeEquivalentTo(new[] { multa, trabalhista });
    }

    [Fact]
    public void Create_DeveNascerSemCobertura_QuandoNadaEscolhido_RN104()
    {
        var group = NewGroup([]);

        group.AdditionalCoverages.Should().BeEmpty();
    }

    [Fact]
    public void UpdateDraft_DeveSubstituirAsCoberturasEscolhidas_RN104()
    {
        var multa = Guid.CreateVersion7();
        var fiscal = Guid.CreateVersion7();
        var group = NewGroup([multa]);

        group.UpdateDraft(
            group.PolicyHolderId, null, group.InsuredId, group.ModalityId, 1_000m,
            new DateOnly(2026, 9, 1), new DateOnly(2026, 12, 1),
            EQuotationScopeMode.All, [], [fiscal]);

        group.AdditionalCoverages.Select(coverage => coverage.AdditionalCoverageId)
            .Should().BeEquivalentTo(new[] { fiscal });
    }

    [Fact]
    public void UpdateDraft_DeveLimparAsCoberturas_QuandoListaVazia_RN104()
    {
        var group = NewGroup([Guid.CreateVersion7()]);

        group.UpdateDraft(
            group.PolicyHolderId, null, group.InsuredId, group.ModalityId, 1_000m,
            new DateOnly(2026, 9, 1), new DateOnly(2026, 12, 1),
            EQuotationScopeMode.All, [], []);

        group.AdditionalCoverages.Should().BeEmpty();
    }

    [Fact]
    public void Create_DeveApontarAsCoberturasParaOGrupo_RN104()
    {
        var multa = Guid.CreateVersion7();

        var group = NewGroup([multa]);

        group.AdditionalCoverages.Should().ContainSingle()
            .Which.QuotationGroupId.Should().Be(group.Id);
    }

    private static QuotationGroup NewGroup(IEnumerable<Guid> coverageIds)
        => QuotationGroup.Create(
            Guid.CreateVersion7(), null, Guid.CreateVersion7(), Guid.CreateVersion7(), 1_000m,
            new DateOnly(2026, 9, 1), new DateOnly(2026, 12, 1),
            EQuotationScopeMode.All, [], coverageIds);
}

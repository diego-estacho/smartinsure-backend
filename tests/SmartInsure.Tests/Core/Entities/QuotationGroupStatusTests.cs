using FluentAssertions;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;

namespace SmartInsure.Tests.Core.Entities;

/// <summary>
/// RN-508 — situações do Grupo de Cotação nesta fase: Rascunho → Cotado → Emissão solicitada. A
/// plataforma não afirma "Emitida": afirma o que sabe, que a emissão foi solicitada. Falha no emitir
/// mantém Cotado — não existe situação intermediária de "emitindo".
/// </summary>
[Trait("RuleId", "RN-508")]
public class QuotationGroupStatusTests
{
    private static QuotationGroup NewGroup()
        => QuotationGroup.Create(
            Guid.CreateVersion7(), null, Guid.CreateVersion7(), Guid.CreateVersion7(), 1_000m,
            new DateOnly(2026, 8, 1), new DateOnly(2027, 8, 1),
            EQuotationScopeMode.All, [], includesPenaltyCoverage: false, includesLaborCoverage: false);

    [Fact]
    public void Grupo_DeveNascerEmRascunho()
    {
        NewGroup().Status.Should().Be(EQuotationGroupStatus.Draft);
    }

    [Fact]
    public void MarkQuoted_DevePromoverRascunhoParaCotado()
    {
        var group = NewGroup();

        group.MarkQuoted();

        group.Status.Should().Be(EQuotationGroupStatus.Quoted);
    }

    [Fact]
    public void MarkQuoted_ChamadoDeNovo_DeveSerIdempotente()
    {
        var group = NewGroup();
        group.MarkQuoted();

        group.MarkQuoted();

        group.Status.Should().Be(EQuotationGroupStatus.Quoted);
    }

    [Fact]
    public void MarkEmissionRequested_DevePromoverCotadoParaEmissaoSolicitada()
    {
        var group = NewGroup();
        group.MarkQuoted();

        group.MarkEmissionRequested();

        group.Status.Should().Be(EQuotationGroupStatus.EmissionRequested);
    }

    [Fact]
    public void MarkEmissionRequested_EmRascunho_DeveSerRecusado()
    {
        var group = NewGroup();

        var act = group.MarkEmissionRequested;

        act.Should().Throw<InvalidOperationException>().WithMessage("*Cotado*");
        group.Status.Should().Be(EQuotationGroupStatus.Draft);
    }

    [Fact]
    public void MarkEmissionRequested_ChamadoDeNovo_DeveSerRecusado()
    {
        var group = NewGroup();
        group.MarkQuoted();
        group.MarkEmissionRequested();

        var act = group.MarkEmissionRequested;

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void UpdateDraft_ComEmissaoSolicitada_DeveSerRecusado()
    {
        var group = NewGroup();
        group.MarkQuoted();
        group.MarkEmissionRequested();

        var act = () => group.UpdateDraft(
            Guid.CreateVersion7(), null, Guid.CreateVersion7(), Guid.CreateVersion7(), 2_000m,
            new DateOnly(2026, 9, 1), new DateOnly(2027, 9, 1),
            EQuotationScopeMode.All, [], includesPenaltyCoverage: true, includesLaborCoverage: false);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void SelectQuotation_ComEmissaoSolicitada_DeveSerRecusado()
    {
        var group = NewGroup();
        group.MarkQuoted();
        group.MarkEmissionRequested();

        var act = () => group.SelectQuotation(Guid.CreateVersion7());

        act.Should().Throw<InvalidOperationException>();
    }
}

using FluentAssertions;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;

namespace SmartInsure.Tests.Core.Entities;

/// <summary>RN-053 — estabelecimento cotado (matriz ou Filial) do Grupo de Cotação.</summary>
public class QuotationGroupTests
{
    private static QuotationGroup NewGroup(Guid? branchPersonId)
        => QuotationGroup.Create(
            Guid.CreateVersion7(),
            branchPersonId,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            500m,
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 2, 1),
            EQuotationScopeMode.All,
            [],
            false,
            false);

    [Fact]
    [Trait("RuleId", "RN-053")]
    public void Create_SemFilial_DeveNascerComEstabelecimentoNulo()
    {
        var group = NewGroup(branchPersonId: null);

        group.BranchPersonId.Should().BeNull();
    }

    [Fact]
    [Trait("RuleId", "RN-053")]
    public void Create_ComFilial_DeveNascerComEstabelecimentoInformado()
    {
        var branchId = Guid.NewGuid();

        var group = NewGroup(branchPersonId: branchId);

        group.BranchPersonId.Should().Be(branchId);
    }

    [Fact]
    [Trait("RuleId", "RN-053")]
    public void UpdateDraft_DeveLimparOEstabelecimentoQuandoNaoInformado()
    {
        var branchId = Guid.NewGuid();
        var group = NewGroup(branchPersonId: branchId);

        group.UpdateDraft(
            Guid.CreateVersion7(),
            branchPersonId: null,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            800m,
            new DateOnly(2026, 3, 1),
            new DateOnly(2026, 4, 1),
            EQuotationScopeMode.All,
            [],
            false,
            false);

        group.BranchPersonId.Should().BeNull();
    }

    [Fact]
    [Trait("RuleId", "RN-053")]
    public void UpdateDraft_DeveAtualizarOEstabelecimentoQuandoInformado()
    {
        var group = NewGroup(branchPersonId: null);
        var branchId = Guid.NewGuid();

        group.UpdateDraft(
            Guid.CreateVersion7(),
            branchId,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            800m,
            new DateOnly(2026, 3, 1),
            new DateOnly(2026, 4, 1),
            EQuotationScopeMode.All,
            [],
            false,
            false);

        group.BranchPersonId.Should().Be(branchId);
    }
}

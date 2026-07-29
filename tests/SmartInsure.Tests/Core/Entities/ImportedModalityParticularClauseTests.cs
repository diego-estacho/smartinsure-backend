using FluentAssertions;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;

namespace SmartInsure.Tests.Core.Entities;

/// <summary>RN-048 — Cláusula particular (identidade por modalidade+id externo, upsert reativa).</summary>
public class ImportedModalityParticularClauseTests
{
    private static readonly Guid ModalityId = Guid.CreateVersion7();

    [Fact]
    [Trait("RuleId", "RN-048")]
    public void Create_DeveNascerAtiva_ComChaveExterna()
    {
        var clause = ImportedModalityParticularClause.Create(ModalityId, "123", "Retenção", "texto", "{}");

        clause.ExternalId.Should().Be("123");
        clause.Name.Should().Be("Retenção");
        clause.Status.Should().Be(EImportedModalityClauseStatus.Active);
    }

    [Fact]
    [Trait("RuleId", "RN-048")]
    public void UpdateFromSource_DeveAtualizarEReativar()
    {
        var clause = ImportedModalityParticularClause.Create(ModalityId, "123", "Old", null, null);
        clause.Deactivate();

        clause.UpdateFromSource("Nova", "t", "{\"a\":1}");

        clause.Name.Should().Be("Nova");
        clause.Status.Should().Be(EImportedModalityClauseStatus.Active);
    }
}

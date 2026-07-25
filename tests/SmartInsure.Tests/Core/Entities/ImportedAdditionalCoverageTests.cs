using FluentAssertions;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;

namespace SmartInsure.Tests.Core.Entities;

/// <summary>RN-041/RN-043/RN-044 — Cobertura Adicional Importada: reflete a fonte, reativa, vincula/ignora.</summary>
public class ImportedAdditionalCoverageTests
{
    private static readonly DateTime Now = new(2026, 7, 23, 5, 0, 0, DateTimeKind.Utc);
    private static readonly Guid ImportedModalityId = Guid.CreateVersion7();

    [Fact]
    [Trait("RuleId", "RN-041")]
    public void Create_DeveNormalizarNomeENascerAtivaSemVinculo()
    {
        var coverage = ImportedAdditionalCoverage.Create(
            ImportedModalityId, "  Multa  ", "11111111-1111-1111-1111-111111111111", 1, true, Now);

        coverage.ImportedModalityId.Should().Be(ImportedModalityId);
        coverage.Name.Should().Be("Multa");
        coverage.SourceUniqueId.Should().Be("11111111-1111-1111-1111-111111111111");
        coverage.InsuredAmountCalculationType.Should().Be(1);
        coverage.AllowManualEdit.Should().BeTrue();
        coverage.Status.Should().Be(EImportedAdditionalCoverageStatus.Active);
        coverage.AdditionalCoverageId.Should().BeNull();
        coverage.IsIgnored.Should().BeFalse();
        coverage.LastImportedAt.Should().Be(Now);
    }

    [Fact]
    [Trait("RuleId", "RN-044")]
    public void UpdateFromSource_DeveReativarEPreservarVinculo()
    {
        var coverage = ImportedAdditionalCoverage.Create(ImportedModalityId, "Multa", "uid-1", 1, false, Now);
        var canonical = Guid.CreateVersion7();
        coverage.LinkTo(canonical);
        coverage.Deactivate();

        var later = Now.AddMinutes(30);
        coverage.UpdateFromSource("uid-2", 2, true, later);

        coverage.Status.Should().Be(EImportedAdditionalCoverageStatus.Active);
        coverage.SourceUniqueId.Should().Be("uid-2");
        coverage.InsuredAmountCalculationType.Should().Be(2);
        coverage.AllowManualEdit.Should().BeTrue();
        coverage.LastImportedAt.Should().Be(later);
        // RN-043: o vínculo manual é preservado na reimportação.
        coverage.AdditionalCoverageId.Should().Be(canonical);
    }

    [Fact]
    [Trait("RuleId", "RN-044")]
    public void Deactivate_DeveSerIdempotente()
    {
        var coverage = ImportedAdditionalCoverage.Create(ImportedModalityId, "Multa", "uid-1", 1, false, Now);

        coverage.Deactivate();
        coverage.Deactivate();

        coverage.Status.Should().Be(EImportedAdditionalCoverageStatus.Inactive);
    }

    [Fact]
    [Trait("RuleId", "RN-043")]
    public void LinkUnlink_DeveVincularEDesvincular()
    {
        var coverage = ImportedAdditionalCoverage.Create(ImportedModalityId, "Multa", "uid-1", 1, false, Now);
        var canonical = Guid.CreateVersion7();

        coverage.LinkTo(canonical);
        coverage.AdditionalCoverageId.Should().Be(canonical);

        coverage.Unlink();
        coverage.AdditionalCoverageId.Should().BeNull();
    }

    [Fact]
    [Trait("RuleId", "RN-043")]
    public void IgnoreRestore_DeveAlternarMarcador()
    {
        var coverage = ImportedAdditionalCoverage.Create(ImportedModalityId, "Multa", "uid-1", 1, false, Now);

        coverage.Ignore();
        coverage.IsIgnored.Should().BeTrue();

        coverage.Restore();
        coverage.IsIgnored.Should().BeFalse();
    }

    [Fact]
    [Trait("RuleId", "RN-041")]
    public void SourceUniqueId_DeveAceitarAusente()
    {
        var coverage = ImportedAdditionalCoverage.Create(ImportedModalityId, "Multa", null, 0, false, Now);

        coverage.SourceUniqueId.Should().BeNull();
        coverage.Status.Should().Be(EImportedAdditionalCoverageStatus.Active);
    }
}

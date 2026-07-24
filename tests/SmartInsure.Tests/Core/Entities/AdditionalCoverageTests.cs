using FluentAssertions;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;

namespace SmartInsure.Tests.Core.Entities;

/// <summary>RN-040 — Cobertura Adicional canônica: curada pelo Administrador, nasce Ativa, renomeia, ativa/inativa.</summary>
public class AdditionalCoverageTests
{
    [Fact]
    [Trait("RuleId", "RN-040")]
    public void Create_DeveNormalizarNomeENascerAtiva()
    {
        var coverage = AdditionalCoverage.Create("  Multa  ");

        coverage.Name.Should().Be("Multa");
        coverage.Status.Should().Be(EAdditionalCoverageStatus.Active);
    }

    [Fact]
    [Trait("RuleId", "RN-040")]
    public void Rename_DeveNormalizarNome()
    {
        var coverage = AdditionalCoverage.Create("Multa");

        coverage.Rename("  Multa Contratual  ");

        coverage.Name.Should().Be("Multa Contratual");
    }

    [Fact]
    [Trait("RuleId", "RN-040")]
    public void DeactivateEActivate_DeveAlternarSituacao()
    {
        var coverage = AdditionalCoverage.Create("Multa");

        coverage.Deactivate();
        coverage.Status.Should().Be(EAdditionalCoverageStatus.Inactive);

        coverage.Activate();
        coverage.Status.Should().Be(EAdditionalCoverageStatus.Active);
    }
}

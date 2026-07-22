using FluentAssertions;
using SmartInsure.Core.Entities;

namespace SmartInsure.Tests.Core.Entities;

/// <summary>RN-030 — Grupo Importado preservado como veio da fonte.</summary>
public class ImportedGroupTests
{
    [Fact]
    [Trait("RuleId", "RN-030")]
    public void Create_DevePreservarDadosDaFonte()
    {
        var insurerId = Guid.CreateVersion7();

        var group = ImportedGroup.Create(insurerId, " grp-uid-1 ", " Judiciais ", " GARANTIA_JUDICIAIS ");

        group.InsurerId.Should().Be(insurerId);
        group.SourceId.Should().Be("grp-uid-1");
        group.Name.Should().Be("Judiciais");
        group.Type.Should().Be("GARANTIA_JUDICIAIS");
    }

    [Fact]
    [Trait("RuleId", "RN-030")]
    public void UpdateFromSource_DeveAtualizarNomeETipo()
    {
        var group = ImportedGroup.Create(Guid.CreateVersion7(), "grp-uid-1", "Judiciais", "GARANTIA_JUDICIAIS");

        group.UpdateFromSource("Judiciais e Recursais", "GARANTIA_RECURSAIS");

        group.Name.Should().Be("Judiciais e Recursais");
        group.Type.Should().Be("GARANTIA_RECURSAIS");
    }
}

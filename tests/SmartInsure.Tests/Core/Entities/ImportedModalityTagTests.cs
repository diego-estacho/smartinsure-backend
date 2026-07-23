using FluentAssertions;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;

namespace SmartInsure.Tests.Core.Entities;

/// <summary>RN-040 — Tag da Modalidade Importada (1:1, nasce Ativa, upsert reativa).</summary>
public class ImportedModalityTagTests
{
    private static readonly Guid ModalityId = Guid.CreateVersion7();

    [Fact]
    [Trait("RuleId", "RN-040")]
    public void Create_DeveNascerAtiva_ComJsonTag()
    {
        var tag = ImportedModalityTag.Create(ModalityId, "{\"campo\":1}", "objeto");

        tag.ImportedModalityId.Should().Be(ModalityId);
        tag.JsonTag.Should().Be("{\"campo\":1}");
        tag.Status.Should().Be(EImportedModalityTagStatus.Active);
    }

    [Fact]
    [Trait("RuleId", "RN-040")]
    public void UpdateFromSource_DeveAtualizarEReativar()
    {
        var tag = ImportedModalityTag.Create(ModalityId, "{\"v\":1}", null);
        tag.Deactivate();

        tag.UpdateFromSource("{\"v\":2}", "novo");

        tag.JsonTag.Should().Be("{\"v\":2}");
        tag.ObjectText.Should().Be("novo");
        tag.Status.Should().Be(EImportedModalityTagStatus.Active);
    }
}

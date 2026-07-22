using FluentAssertions;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;

namespace SmartInsure.Tests.Core.Entities;

/// <summary>RN-032 — Mapeamento automático por identificador do motor nasce Confirmado.</summary>
public class ModalityMappingTests
{
    [Fact]
    [Trait("RuleId", "RN-032")]
    public void CreateByIdentifier_DeveNascerConfirmadoPorIdentificador()
    {
        var importedId = Guid.CreateVersion7();
        var modalityId = Guid.CreateVersion7();

        var mapping = ModalityMapping.CreateByIdentifier(importedId, modalityId);

        mapping.ImportedModalityId.Should().Be(importedId);
        mapping.ModalityId.Should().Be(modalityId);
        mapping.Establishment.Should().Be(EMappingEstablishment.Identifier);
        mapping.Status.Should().Be(EModalityMappingStatus.Confirmed);
        mapping.Confidence.Should().BeNull();
    }
}

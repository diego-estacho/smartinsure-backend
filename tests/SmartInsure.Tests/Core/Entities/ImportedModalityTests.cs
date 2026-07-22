using FluentAssertions;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;

namespace SmartInsure.Tests.Core.Entities;

/// <summary>RN-030/RN-035 — Modalidade Importada reflete a fonte; inativa/reativa pela importação.</summary>
public class ImportedModalityTests
{
    private static readonly DateTime Now = new(2026, 7, 22, 3, 0, 0, DateTimeKind.Utc);

    private static ImportedModality New(
        EImportedModalityStatus _ = EImportedModalityStatus.Active)
        => ImportedModality.Create(
            insurerId: Guid.CreateVersion7(),
            sourceId: "mod-uid-1",
            originName: " Garantia de Execução ",
            branch: ESuretyBranch.Public,
            engineModalityId: "12",
            engineModalityName: "Execução",
            importedGroupId: Guid.CreateVersion7(),
            commercialParameters: "{\"MaxPeriodInDays\":720}",
            lastImportedAt: Now);

    [Fact]
    [Trait("RuleId", "RN-030")]
    public void Create_DeveNascerAtivaComDadosDaFonte()
    {
        var groupId = Guid.CreateVersion7();
        var modality = ImportedModality.Create(
            Guid.CreateVersion7(), "mod-uid-1", " Garantia ", ESuretyBranch.Private,
            "12", "Execução", groupId, "{\"a\":1}", Now);

        modality.OriginName.Should().Be("Garantia");
        modality.Branch.Should().Be(ESuretyBranch.Private);
        modality.EngineModalityId.Should().Be("12");
        modality.ImportedGroupId.Should().Be(groupId);
        modality.CommercialParameters.Should().Be("{\"a\":1}");
        modality.LastImportedAt.Should().Be(Now);
        modality.Status.Should().Be(EImportedModalityStatus.Active);
    }

    [Fact]
    [Trait("RuleId", "RN-030")]
    public void UpdateFromSource_DeveAtualizarDadosEManterAtiva()
    {
        var modality = New();
        var later = Now.AddDays(1);
        var newGroup = Guid.CreateVersion7();

        modality.UpdateFromSource(
            "Garantia Recursal", ESuretyBranch.Private, "15", "Recursal", newGroup, "{\"b\":2}", later);

        modality.OriginName.Should().Be("Garantia Recursal");
        modality.Branch.Should().Be(ESuretyBranch.Private);
        modality.EngineModalityId.Should().Be("15");
        modality.ImportedGroupId.Should().Be(newGroup);
        modality.CommercialParameters.Should().Be("{\"b\":2}");
        modality.LastImportedAt.Should().Be(later);
        modality.Status.Should().Be(EImportedModalityStatus.Active);
    }

    [Fact]
    [Trait("RuleId", "RN-035")]
    public void UpdateFromSource_DeveReativar_QuandoReapareceInativa()
    {
        var modality = New();
        modality.Deactivate();
        modality.Status.Should().Be(EImportedModalityStatus.Inactive);

        modality.UpdateFromSource(
            "Garantia", ESuretyBranch.Public, "12", "Execução", null, null, Now.AddDays(2));

        modality.Status.Should().Be(EImportedModalityStatus.Active);
    }

    [Fact]
    [Trait("RuleId", "RN-035")]
    public void Deactivate_DeveInativar_EhIdempotente()
    {
        var modality = New();

        modality.Deactivate();
        modality.Deactivate(); // idempotente: importação automática não deve lançar

        modality.Status.Should().Be(EImportedModalityStatus.Inactive);
    }

    [Fact]
    [Trait("RuleId", "RN-032")]
    public void LinkToModality_DeveVincularAutomaticamente_QuandoSemVinculo()
    {
        var modality = New();
        var modalityId = Guid.CreateVersion7();

        modality.LinkToModality(modalityId, EModalityLinkSource.Automatic);

        modality.ModalityId.Should().Be(modalityId);
        modality.ModalityLinkSource.Should().Be(EModalityLinkSource.Automatic);
    }

    [Fact]
    [Trait("RuleId", "RN-034")]
    public void LinkToModality_NaoDeveSobrescreverManual_QuandoImportacaoAutomatica()
    {
        var modality = New();
        var manualTarget = Guid.CreateVersion7();
        modality.LinkToModality(manualTarget, EModalityLinkSource.Manual);

        // Reimportação automática não pode desfazer o override manual (RN-032/RN-034).
        modality.LinkToModality(Guid.CreateVersion7(), EModalityLinkSource.Automatic);

        modality.ModalityId.Should().Be(manualTarget);
        modality.ModalityLinkSource.Should().Be(EModalityLinkSource.Manual);
    }

    [Fact]
    [Trait("RuleId", "RN-034")]
    public void LinkToModality_DevePermitirOverrideManual_SobreVinculoAutomatico()
    {
        var modality = New();
        modality.LinkToModality(Guid.CreateVersion7(), EModalityLinkSource.Automatic);
        var manualTarget = Guid.CreateVersion7();

        modality.LinkToModality(manualTarget, EModalityLinkSource.Manual);

        modality.ModalityId.Should().Be(manualTarget);
        modality.ModalityLinkSource.Should().Be(EModalityLinkSource.Manual);
    }
}

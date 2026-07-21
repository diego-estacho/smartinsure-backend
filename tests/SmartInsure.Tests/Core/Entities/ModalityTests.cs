using FluentAssertions;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Tests.Core.Entities;

/// <summary>RN-029/RN-036 — criação, edição e transições de situação da Modalidade.</summary>
public class ModalityTests
{
    private static Modality NewModality(EModalityStatus status = EModalityStatus.Active)
        => Modality.Create(" Garantia de Execução de Contrato ", Guid.CreateVersion7(), " Performance ", status);

    [Fact]
    [Trait("RuleId", "RN-029")]
    public void Create_DeveNormalizarDadosENascerComGrupoESituacao_QuandoDadosValidos()
    {
        var groupId = Guid.CreateVersion7();

        var modality = Modality.Create(" Garantia de Execução de Contrato ", groupId, " Performance ", EModalityStatus.Active);

        modality.Name.Should().Be("Garantia de Execução de Contrato");
        modality.ModalityGroupId.Should().Be(groupId);
        modality.Description.Should().Be("Performance");
        modality.Status.Should().Be(EModalityStatus.Active);
    }

    [Fact]
    [Trait("RuleId", "RN-029")]
    public void Create_DeveAceitarDescricaoAusente_QuandoDescricaoVazia()
    {
        var modality = Modality.Create("Garantia Judicial", Guid.CreateVersion7(), null, EModalityStatus.Active);

        modality.Description.Should().BeNull();
    }

    [Fact]
    [Trait("RuleId", "RN-029")]
    public void Update_DeveAlterarDadosEGrupoSemMudarSituacao()
    {
        var modality = NewModality(EModalityStatus.Inactive);
        var newGroupId = Guid.CreateVersion7();

        modality.Update("Garantia de Adiantamento de Pagamento", newGroupId, "Advance payment");

        modality.Name.Should().Be("Garantia de Adiantamento de Pagamento");
        modality.ModalityGroupId.Should().Be(newGroupId);
        modality.Description.Should().Be("Advance payment");
        modality.Status.Should().Be(EModalityStatus.Inactive);
    }

    [Fact]
    [Trait("RuleId", "RN-036")]
    public void Activate_DeveRecusar_QuandoJaAtiva()
    {
        var modality = NewModality();

        var act = modality.Activate;

        act.Should().Throw<ConflictException>();
    }

    [Fact]
    [Trait("RuleId", "RN-036")]
    public void Deactivate_DeveRecusar_QuandoJaInativa()
    {
        var modality = NewModality(EModalityStatus.Inactive);

        var act = modality.Deactivate;

        act.Should().Throw<ConflictException>();
    }
}

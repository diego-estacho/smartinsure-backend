using FluentAssertions;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Tests.Core.Entities;

/// <summary>RN-029/RN-036 — criação (manual e derivada da Global), edição e transições de situação.</summary>
public class ModalityTests
{
    private static Modality NewManual(EModalityStatus status = EModalityStatus.Active)
        => Modality.CreateManual(" Garantia de Execução de Contrato ", " Performance ", status);

    [Fact]
    [Trait("RuleId", "RN-029")]
    public void CreateManual_DeveNormalizarDadosENascerSemIdGlobal_QuandoDadosValidos()
    {
        var modality = Modality.CreateManual(" Garantia de Execução de Contrato ", " Performance ", EModalityStatus.Active);

        modality.Name.Should().Be("Garantia de Execução de Contrato");
        modality.Description.Should().Be("Performance");
        modality.Status.Should().Be(EModalityStatus.Active);
        modality.GlobalModalityExternalId.Should().BeNull();
    }

    [Fact]
    [Trait("RuleId", "RN-029")]
    public void CreateManual_DeveAceitarDescricaoAusente_QuandoDescricaoVazia()
    {
        var modality = Modality.CreateManual("Garantia Judicial", null, EModalityStatus.Active);

        modality.Description.Should().BeNull();
    }

    [Fact]
    [Trait("RuleId", "RN-032")]
    public void CreateFromGlobal_DeveNascerAtivaComIdGlobalENomeDaFonte()
    {
        var modality = Modality.CreateFromGlobal(" 12 ", " Garantia Judicial ");

        modality.GlobalModalityExternalId.Should().Be("12");
        modality.Name.Should().Be("Garantia Judicial");
        modality.Status.Should().Be(EModalityStatus.Active);
        modality.Description.Should().BeNull();
    }

    [Fact]
    [Trait("RuleId", "RN-029")]
    public void Update_DeveAlterarNomeEDescricaoSemMudarSituacao()
    {
        var modality = NewManual(EModalityStatus.Inactive);

        modality.Update("Garantia de Adiantamento de Pagamento", "Advance payment");

        modality.Name.Should().Be("Garantia de Adiantamento de Pagamento");
        modality.Description.Should().Be("Advance payment");
        modality.Status.Should().Be(EModalityStatus.Inactive);
    }

    [Fact]
    [Trait("RuleId", "RN-036")]
    public void Activate_DeveRecusar_QuandoJaAtiva()
    {
        var modality = NewManual();

        var act = modality.Activate;

        act.Should().Throw<ConflictException>();
    }

    [Fact]
    [Trait("RuleId", "RN-036")]
    public void Deactivate_DeveRecusar_QuandoJaInativa()
    {
        var modality = NewManual(EModalityStatus.Inactive);

        var act = modality.Deactivate;

        act.Should().Throw<ConflictException>();
    }
}

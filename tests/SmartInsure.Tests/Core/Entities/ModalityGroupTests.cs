using FluentAssertions;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Tests.Core.Entities;

/// <summary>RN-029/RN-036 — criação, edição e transições de situação do Grupo de Modalidade.</summary>
public class ModalityGroupTests
{
    private static ModalityGroup NewGroup(EModalityGroupStatus status = EModalityGroupStatus.Active)
        => ModalityGroup.Create(" Garantias de Licitação ", " Bid bonds ", 1, status);

    [Fact]
    [Trait("RuleId", "RN-029")]
    public void Create_DeveNormalizarDadosENascerComOrdemESituacao_QuandoDadosValidos()
    {
        var group = ModalityGroup.Create(" Garantias de Licitação ", " Bid bonds ", 3, EModalityGroupStatus.Active);

        group.Name.Should().Be("Garantias de Licitação");
        group.Description.Should().Be("Bid bonds");
        group.DisplayOrder.Should().Be(3);
        group.Status.Should().Be(EModalityGroupStatus.Active);
    }

    [Fact]
    [Trait("RuleId", "RN-029")]
    public void Create_DeveAceitarDescricaoAusente_QuandoDescricaoVazia()
    {
        var group = ModalityGroup.Create("Garantias Judiciais", "  ", 0, EModalityGroupStatus.Active);

        group.Description.Should().BeNull();
    }

    [Fact]
    [Trait("RuleId", "RN-029")]
    public void Update_DeveAlterarDadosSemMudarSituacao()
    {
        var group = NewGroup(EModalityGroupStatus.Inactive);

        group.Update("Garantias de Contrato", "Performance bonds", 5);

        group.Name.Should().Be("Garantias de Contrato");
        group.Description.Should().Be("Performance bonds");
        group.DisplayOrder.Should().Be(5);
        group.Status.Should().Be(EModalityGroupStatus.Inactive);
    }

    [Fact]
    [Trait("RuleId", "RN-036")]
    public void Deactivate_DeveTornarInativo_QuandoAtivo()
    {
        var group = NewGroup();

        group.Deactivate();

        group.Status.Should().Be(EModalityGroupStatus.Inactive);
    }

    [Fact]
    [Trait("RuleId", "RN-036")]
    public void Activate_DeveTornarAtivo_QuandoInativo()
    {
        var group = NewGroup(EModalityGroupStatus.Inactive);

        group.Activate();

        group.Status.Should().Be(EModalityGroupStatus.Active);
    }

    [Fact]
    [Trait("RuleId", "RN-036")]
    public void Activate_DeveRecusar_QuandoJaAtivo()
    {
        var group = NewGroup();

        var act = group.Activate;

        act.Should().Throw<ConflictException>();
    }

    [Fact]
    [Trait("RuleId", "RN-036")]
    public void Deactivate_DeveRecusar_QuandoJaInativo()
    {
        var group = NewGroup(EModalityGroupStatus.Inactive);

        var act = group.Deactivate;

        act.Should().Throw<ConflictException>();
    }
}

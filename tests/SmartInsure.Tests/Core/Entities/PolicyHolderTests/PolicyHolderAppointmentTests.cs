using FluentAssertions;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;
using Xunit;

namespace SmartInsure.Tests.Core.Entities.PolicyHolderTests;

[Trait("Category", "Domain")]
public sealed class PolicyHolderAppointmentTests
{
    [Fact]
    [Trait("RuleId", "RN-027")]
    public void Create_DeveNascerVigenteComStartedAtAgora_QuandoCriada()
    {
        var policyHolderId = Guid.NewGuid();
        var brokerageId = Guid.NewGuid();
        var insurerId = Guid.NewGuid();
        var beforeCreate = DateTime.UtcNow;

        var appointment = PolicyHolderAppointment.Create(policyHolderId, brokerageId, insurerId);
        var afterCreate = DateTime.UtcNow;

        appointment.PolicyHolderId.Should().Be(policyHolderId);
        appointment.BrokerageId.Should().Be(brokerageId);
        appointment.InsurerId.Should().Be(insurerId);
        appointment.Status.Should().Be(EPolicyHolderAppointmentStatus.Active);
        appointment.StartedAt.Should().BeOnOrAfter(beforeCreate);
        appointment.StartedAt.Should().BeOnOrBefore(afterCreate);
        appointment.EndedAt.Should().BeNull();
    }

    [Fact]
    [Trait("RuleId", "RN-028")]
    public void End_DevePassarParaEndedComEndedAtAgora_QuandoVigente()
    {
        var appointment = PolicyHolderAppointment.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        var beforeEnd = DateTime.UtcNow;

        appointment.End();
        var afterEnd = DateTime.UtcNow;

        appointment.Status.Should().Be(EPolicyHolderAppointmentStatus.Ended);
        appointment.EndedAt.Should().NotBeNull();
        appointment.EndedAt.Should().BeOnOrAfter(beforeEnd);
        appointment.EndedAt.Should().BeOnOrBefore(afterEnd);
    }

    [Fact]
    [Trait("RuleId", "RN-028")]
    public void End_DeveLançarConflictException_QuandoJaEncerrada()
    {
        var appointment = PolicyHolderAppointment.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        appointment.End();

        var action = () => appointment.End();

        action.Should().ThrowExactly<ConflictException>()
            .WithMessage("A nomeação já está encerrada.");
    }

    [Fact]
    [Trait("RuleId", "RN-028")]
    public void End_NuncaDeveVoltarAActive_QuandoEncerrada()
    {
        var appointment = PolicyHolderAppointment.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        appointment.End();

        appointment.Status.Should().Be(EPolicyHolderAppointmentStatus.Ended);
        // Verificar que não existe método para reativar
        typeof(PolicyHolderAppointment).GetMethod("Activate")
            .Should().BeNull("Nomeação encerrada nunca volta a Active");
    }
}

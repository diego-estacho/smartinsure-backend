using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Core.Entities;

/// <summary>
/// Nomeação de Tomador (RN-027/RN-028): vínculo entre um Tomador (Pessoa com papel
/// PolicyHolder), uma Corretora (Person com papel Broker) e uma Seguradora. Um Tomador
/// pode ter no máximo uma Nomeação Vigente (Active) por Seguradora; a substituição ou
/// encerramento modifica apenas essa nomeação. Uma vez Ended, nunca volta a Active.
/// </summary>
public sealed class PolicyHolderAppointment : EntityBase
{
    private PolicyHolderAppointment()
    {
    }

    public Guid PolicyHolderId { get; private set; }

    public Guid BrokerageId { get; private set; }

    public Guid InsurerId { get; private set; }

    public EPolicyHolderAppointmentStatus Status { get; private set; }

    public DateTime StartedAt { get; private set; }

    public DateTime? EndedAt { get; private set; }

    /// <summary>RN-027: a Nomeação nasce Vigente (Active) com data de início = agora.</summary>
    public static PolicyHolderAppointment Create(
        Guid policyHolderId,
        Guid brokerageId,
        Guid insurerId)
        => new()
        {
            PolicyHolderId = policyHolderId,
            BrokerageId = brokerageId,
            InsurerId = insurerId,
            Status = EPolicyHolderAppointmentStatus.Active,
            StartedAt = DateTime.UtcNow,
        };

    /// <summary>
    /// RN-028: encerra a Nomeação Vigente (Active → Ended + data de fim = agora).
    /// Nomeação já Ended não pode ser encerrada novamente.
    /// </summary>
    public void End()
    {
        if (Status == EPolicyHolderAppointmentStatus.Ended)
        {
            throw new ConflictException("A nomeação já está encerrada.");
        }

        Status = EPolicyHolderAppointmentStatus.Ended;
        EndedAt = DateTime.UtcNow;
    }
}

using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Core.Entities;

/// <summary>
/// Vínculo de papel da Pessoa (RN-017): uma Pessoa acumula papéis
/// (Insured/Broker/PolicyHolder); o vínculo nunca duplica e não é removido nesta fase.
/// O papel Corretor carrega situação Ativa/Inativa para a jornada Corretoras (RN-018/RN-021).
/// </summary>
public sealed class PersonRole : EntityBase
{
    private PersonRole()
    {
    }

    public Guid PersonId { get; private set; }

    public EPersonRole Role { get; private set; }

    public EPersonRoleStatus Status { get; private set; }

    public void Activate()
    {
        if (Status == EPersonRoleStatus.Active)
        {
            throw new ConflictException("A corretora já está ativa.");
        }

        Status = EPersonRoleStatus.Active;
    }

    public void Deactivate()
    {
        if (Status == EPersonRoleStatus.Inactive)
        {
            throw new ConflictException("A corretora já está inativa.");
        }

        Status = EPersonRoleStatus.Inactive;
    }

    internal static PersonRole Create(Guid personId, EPersonRole role)
        => new()
        {
            PersonId = personId,
            Role = role,
            Status = EPersonRoleStatus.Active,
        };
}

using SmartInsure.Core.Enumerators;

namespace SmartInsure.Core.Entities;

/// <summary>
/// Vínculo de papel da Pessoa (RN-017): uma Pessoa acumula papéis
/// (Insured/Broker/PolicyHolder); o vínculo nunca duplica e não é removido nesta fase.
/// </summary>
public sealed class PersonRole : EntityBase
{
    private PersonRole()
    {
    }

    public Guid PersonId { get; private set; }

    public EPersonRole Role { get; private set; }

    internal static PersonRole Create(Guid personId, EPersonRole role)
        => new()
        {
            PersonId = personId,
            Role = role,
        };
}

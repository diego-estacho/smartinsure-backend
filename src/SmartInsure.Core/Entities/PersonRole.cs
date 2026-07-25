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

    /// <summary>RN-054: contato complementar da Corretora — só o papel Corretor usa (nulo nos demais).</summary>
    public string? ContactEmail { get; private set; }

    public string? ContactPhone { get; private set; }

    public string? ResponsibleName { get; private set; }

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

    /// <summary>RN-054: edita os dados de contato complementares do papel Corretor.</summary>
    public void UpdateBrokerageContact(
        string? contactEmail,
        string? contactPhone,
        string? responsibleName)
    {
        ContactEmail = Normalize(contactEmail);
        ContactPhone = Normalize(contactPhone);
        ResponsibleName = Normalize(responsibleName);
    }

    internal static PersonRole Create(Guid personId, EPersonRole role)
        => new()
        {
            PersonId = personId,
            Role = role,
            Status = EPersonRoleStatus.Active,
        };

    /// <summary>RN-019: papel Corretor criado na confirmação, com situação inicial e contato.</summary>
    internal static PersonRole CreateBroker(
        Guid personId,
        bool active,
        string? contactEmail,
        string? contactPhone,
        string? responsibleName)
        => new()
        {
            PersonId = personId,
            Role = EPersonRole.Broker,
            Status = active ? EPersonRoleStatus.Active : EPersonRoleStatus.Inactive,
            ContactEmail = Normalize(contactEmail),
            ContactPhone = Normalize(contactPhone),
            ResponsibleName = Normalize(responsibleName),
        };

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

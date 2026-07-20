using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Core.Entities;

/// <summary>
/// Pessoa (glossário): física (CPF) ou jurídica (CNPJ), única por documento,
/// cadastrada uma vez e reaproveitada pelos papéis que a referenciam (RN-013/RN-014).
/// Natureza Jurídica só existe para a jurídica (RN-015); os dados não são
/// atualizados pelo fluxo de busca (RN-014).
/// </summary>
public sealed class Person : EntityBase
{
    private readonly List<PersonAddress> _addresses = [];
    private readonly List<PersonRole> _roles = [];

    private Person()
    {
    }

    public string DocumentNumber { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string? SocialName { get; private set; }

    public EPersonType Type { get; private set; }

    public Guid? LegalNatureId { get; private set; }

    public LegalNature? LegalNature { get; private set; }

    public IReadOnlyCollection<PersonAddress> Addresses => _addresses.AsReadOnly();

    public IReadOnlyCollection<PersonRole> Roles => _roles.AsReadOnly();

    public static Person Create(
        string documentNumber,
        string name,
        string? socialName,
        Guid? legalNatureId)
    {
        var digits = new string([.. documentNumber.Where(char.IsDigit)]);

        var type = digits.Length switch
        {
            11 => EPersonType.F,
            14 => EPersonType.J,
            _ => throw new BusinessRuleException(
                "O documento da pessoa deve ser um CPF (11 dígitos) ou um CNPJ (14 dígitos)."),
        };

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new BusinessRuleException("O nome da pessoa é obrigatório.");
        }

        if (type == EPersonType.J && legalNatureId is null)
        {
            throw new BusinessRuleException("A pessoa jurídica exige natureza jurídica catalogada.");
        }

        if (type == EPersonType.F && legalNatureId is not null)
        {
            throw new BusinessRuleException("A pessoa física não possui natureza jurídica.");
        }

        return new Person
        {
            DocumentNumber = digits,
            Name = name.Trim(),
            SocialName = string.IsNullOrWhiteSpace(socialName) ? null : socialName.Trim(),
            Type = type,
            LegalNatureId = legalNatureId,
        };
    }

    /// <summary>RN-014: o endereço retornado pelo Birô é gravado como endereço principal.</summary>
    public void AddMainAddress(
        string? zipCode,
        string? street,
        string? number,
        string? complement,
        string? neighborhood,
        string? city,
        string? state)
    {
        foreach (var address in _addresses)
        {
            if (address.IsMain)
            {
                throw new ConflictException("A pessoa já possui endereço principal.");
            }
        }

        _addresses.Add(PersonAddress.CreateMain(
            Id, zipCode, street, number, complement, neighborhood, city, state));
    }

    /// <summary>RN-017: vínculo de papel acumulável e idempotente — repetir não duplica.</summary>
    public void AssignRole(EPersonRole role)
    {
        foreach (var existing in _roles)
        {
            if (existing.Role == role)
            {
                return;
            }
        }

        _roles.Add(PersonRole.Create(Id, role));
    }

    public PersonRole? GetRole(EPersonRole role)
    {
        foreach (var existing in _roles)
        {
            if (existing.Role == role)
            {
                return existing;
            }
        }

        return null;
    }

    /// <summary>RN-016: matriz é o estabelecimento de ordem /0001 do CNPJ (só pessoa jurídica).</summary>
    public bool IsHeadquarters
        => Type == EPersonType.J && DocumentNumber[8..12] == "0001";

    /// <summary>RN-026: adiciona endereço complementar (não principal).</summary>
    public void AddAdditionalAddress(
        string? zipCode,
        string? street,
        string? number,
        string? complement,
        string? neighborhood,
        string? city,
        string? state)
    {
        _addresses.Add(PersonAddress.CreateAdditional(
            Id, zipCode, street, number, complement, neighborhood, city, state));
    }

    /// <summary>RN-026: altera endereço complementar (nunca o principal).</summary>
    public void UpdateAdditionalAddress(
        Guid addressId,
        string? zipCode,
        string? street,
        string? number,
        string? complement,
        string? neighborhood,
        string? city,
        string? state)
    {
        PersonAddress? address = FindAddressById(addressId);

        if (address is null)
        {
            throw new NotFoundException("Endereço não encontrado.");
        }

        if (address.IsMain)
        {
            throw new BusinessRuleException("O endereço principal não pode ser alterado ou removido.");
        }

        address.Update(zipCode, street, number, complement, neighborhood, city, state);
    }

    /// <summary>RN-026: remove endereço complementar (nunca o principal).</summary>
    public void RemoveAdditionalAddress(Guid addressId)
    {
        PersonAddress? address = FindAddressById(addressId);

        if (address is null)
        {
            throw new NotFoundException("Endereço não encontrado.");
        }

        if (address.IsMain)
        {
            throw new BusinessRuleException("O endereço principal não pode ser alterado ou removido.");
        }

        _addresses.Remove(address);
    }

    private PersonAddress? FindAddressById(Guid addressId)
    {
        foreach (var address in _addresses)
        {
            if (address.Id == addressId)
            {
                return address;
            }
        }

        return null;
    }
}

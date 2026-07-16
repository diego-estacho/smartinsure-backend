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

    public static Person Create(
        string documentNumber,
        string name,
        string? socialName,
        Guid? legalNatureId)
    {
        var digits = new string([.. documentNumber.Where(char.IsDigit)]);

        var type = digits.Length switch
        {
            11 => EPersonType.Natural,
            14 => EPersonType.Legal,
            _ => throw new BusinessRuleException(
                "O documento da pessoa deve ser um CPF (11 dígitos) ou um CNPJ (14 dígitos)."),
        };

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new BusinessRuleException("O nome da pessoa é obrigatório.");
        }

        if (type == EPersonType.Legal && legalNatureId is null)
        {
            throw new BusinessRuleException("A pessoa jurídica exige natureza jurídica catalogada.");
        }

        if (type == EPersonType.Natural && legalNatureId is not null)
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

    /// <summary>RN-016: matriz é o estabelecimento de ordem /0001 do CNPJ (só pessoa jurídica).</summary>
    public bool IsHeadquarters
        => Type == EPersonType.Legal && DocumentNumber[8..12] == "0001";
}

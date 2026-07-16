using SmartInsure.Core.Exceptions;

namespace SmartInsure.Core.Entities;

/// <summary>
/// Pessoa (glossário): única por CNPJ, cadastrada uma vez e reaproveitada pelos
/// papéis que a referenciam (RN-013/RN-014). Importada do Birô com endereço principal;
/// os dados não são atualizados pelo fluxo de busca (RN-014).
/// </summary>
public sealed class Person : EntityBase
{
    private readonly List<PersonAddress> _addresses = [];

    private Person()
    {
    }

    public string Cnpj { get; private set; } = string.Empty;

    public string CorporateName { get; private set; } = string.Empty;

    public string? TradeName { get; private set; }

    public Guid LegalNatureId { get; private set; }

    public LegalNature? LegalNature { get; private set; }

    public IReadOnlyCollection<PersonAddress> Addresses => _addresses.AsReadOnly();

    public static Person Create(
        string cnpj,
        string corporateName,
        string? tradeName,
        Guid legalNatureId)
    {
        var digits = new string([.. cnpj.Where(char.IsDigit)]);

        if (digits.Length != 14)
        {
            throw new BusinessRuleException("O CNPJ da pessoa deve ter 14 dígitos.");
        }

        if (string.IsNullOrWhiteSpace(corporateName))
        {
            throw new BusinessRuleException("A razão social da pessoa é obrigatória.");
        }

        return new Person
        {
            Cnpj = digits,
            CorporateName = corporateName.Trim(),
            TradeName = string.IsNullOrWhiteSpace(tradeName) ? null : tradeName.Trim(),
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

    /// <summary>RN-016: matriz é o estabelecimento de ordem /0001 no CNPJ.</summary>
    public bool IsHeadquarters => Cnpj.Length == 14 && Cnpj[8..12] == "0001";
}

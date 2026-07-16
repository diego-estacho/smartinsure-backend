namespace SmartInsure.Core.Entities;

/// <summary>
/// Endereço da Pessoa Jurídica (RN-014): o endereço retornado pelo Birô entra como
/// principal; campos ausentes na fonte permanecem nulos.
/// </summary>
public sealed class LegalEntityAddress : EntityBase
{
    private LegalEntityAddress()
    {
    }

    public Guid LegalEntityId { get; private set; }

    public string? ZipCode { get; private set; }

    public string? Street { get; private set; }

    public string? Number { get; private set; }

    public string? Complement { get; private set; }

    public string? Neighborhood { get; private set; }

    public string? City { get; private set; }

    public string? State { get; private set; }

    public bool IsMain { get; private set; }

    internal static LegalEntityAddress CreateMain(
        Guid legalEntityId,
        string? zipCode,
        string? street,
        string? number,
        string? complement,
        string? neighborhood,
        string? city,
        string? state)
        => new()
        {
            LegalEntityId = legalEntityId,
            ZipCode = Clean(zipCode, digitsOnly: true),
            Street = Clean(street),
            Number = Clean(number),
            Complement = Clean(complement),
            Neighborhood = Clean(neighborhood),
            City = Clean(city),
            State = Clean(state)?.ToUpperInvariant(),
            IsMain = true,
        };

    private static string? Clean(string? value, bool digitsOnly = false)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        return digitsOnly ? new string([.. trimmed.Where(char.IsDigit)]) : trimmed;
    }
}

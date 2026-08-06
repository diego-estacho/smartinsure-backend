namespace SmartInsure.Core.Entities;

/// <summary>
/// Endereço do Segurado da oferta (RN-503): réplica, feita no Grupo de Cotação, do endereço do Segurado
/// escolhido pelo corretor. É esta cópia — não o cadastro da Pessoa — que abastece a emissão, para que
/// alteração posterior no cadastro não mude, sozinha, o que já foi combinado na oferta. A fonte segue
/// sendo o cadastro do Segurado: quando o corretor confirma o endereço de novo, a réplica é atualizada.
/// </summary>
public sealed class QuotationAddress : EntityBase
{
    private QuotationAddress()
    {
    }

    public Guid QuotationGroupId { get; private set; }

    public string? ZipCode { get; private set; }

    public string? Street { get; private set; }

    public string? Number { get; private set; }

    public string? Complement { get; private set; }

    public string? Neighborhood { get; private set; }

    public string? City { get; private set; }

    public string? State { get; private set; }

    internal static QuotationAddress Replicate(
        Guid quotationGroupId,
        string? zipCode,
        string? street,
        string? number,
        string? complement,
        string? neighborhood,
        string? city,
        string? state)
    {
        var address = new QuotationAddress { QuotationGroupId = quotationGroupId };
        address.Update(zipCode, street, number, complement, neighborhood, city, state);

        return address;
    }

    internal void Update(
        string? zipCode,
        string? street,
        string? number,
        string? complement,
        string? neighborhood,
        string? city,
        string? state)
    {
        ZipCode = Trim(zipCode);
        Street = Trim(street);
        Number = Trim(number);
        Complement = Trim(complement);
        Neighborhood = Trim(neighborhood);
        City = Trim(city);
        State = Trim(state);
    }

    /// <summary>
    /// RN-503: a Seguradora exige o endereço para emitir — réplica sem CEP, logradouro, cidade ou UF não
    /// serve, e a correção é feita no cadastro do Segurado, não na oferta.
    /// </summary>
    internal bool IsUsableForIssuance()
        => !string.IsNullOrWhiteSpace(ZipCode)
           && !string.IsNullOrWhiteSpace(Street)
           && !string.IsNullOrWhiteSpace(City)
           && !string.IsNullOrWhiteSpace(State);

    private static string? Trim(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

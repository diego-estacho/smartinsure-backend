namespace SmartInsure.Core.Abstractions.Services.Dtos;

/// <summary>
/// Dados cadastrais retornados pelo Bureau (glossário) para um CPF/CNPJ.
/// O uso de negócio destes dados está em aberto (OPEN-04); o DTO expõe o
/// retorno sem aplicar qualquer efeito automático.
/// </summary>
public sealed record BureauPersonComplement
{
    public string? Document { get; init; }

    public string? Name { get; init; }

    public string? TradeName { get; init; }

    /// <summary>Situação cadastral na fonte (ex.: ATIVA), sem tradução nem efeito automático.</summary>
    public string? RegistrationStatus { get; init; }

    public string? RegistrationStatusDate { get; init; }

    public string? OpeningDate { get; init; }

    public string? LegalNature { get; init; }

    public string? CompanySize { get; init; }

    public string? ShareCapital { get; init; }

    public string? Street { get; init; }

    public string? Number { get; init; }

    public string? AddressComplement { get; init; }

    public string? District { get; init; }

    public string? City { get; init; }

    public string? State { get; init; }

    public string? ZipCode { get; init; }

    public string? Phone { get; init; }

    public string? Email { get; init; }

    public IReadOnlyList<BureauEconomicActivity> MainActivities { get; init; } = [];

    public IReadOnlyList<BureauEconomicActivity> SecondaryActivities { get; init; } = [];
}

/// <summary>Atividade econômica (CNAE) informada pelo Bureau.</summary>
public sealed record BureauEconomicActivity(string? Code, string? Description);

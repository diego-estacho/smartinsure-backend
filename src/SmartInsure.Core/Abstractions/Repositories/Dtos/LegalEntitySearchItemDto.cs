namespace SmartInsure.Core.Abstractions.Repositories.Dtos;

/// <summary>
/// Item de busca de Pessoa Jurídica (RN-013), com a classificação da Natureza
/// Jurídica (RN-015) e o endereço principal quando existente.
/// </summary>
public sealed record LegalEntitySearchItemDto(
    Guid Id,
    string Cnpj,
    string CorporateName,
    string? TradeName,
    bool IsPrivateSector,
    LegalEntityMainAddressDto? MainAddress);

public sealed record LegalEntityMainAddressDto(
    string? ZipCode,
    string? Street,
    string? Number,
    string? Complement,
    string? Neighborhood,
    string? City,
    string? State);

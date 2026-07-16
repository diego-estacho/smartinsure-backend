namespace SmartInsure.Core.Abstractions.Repositories.Dtos;

/// <summary>
/// Item de busca de Pessoa (RN-013), com a classificação da Natureza
/// Jurídica (RN-015) e o endereço principal quando existente.
/// </summary>
public sealed record PersonSearchItemDto(
    Guid Id,
    string Cnpj,
    string CorporateName,
    string? TradeName,
    bool IsPrivateSector,
    PersonMainAddressDto? MainAddress);

public sealed record PersonMainAddressDto(
    string? ZipCode,
    string? Street,
    string? Number,
    string? Complement,
    string? Neighborhood,
    string? City,
    string? State);

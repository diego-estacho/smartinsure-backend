namespace SmartInsure.Core.Abstractions.Repositories.Dtos;

/// <summary>
/// Item de busca de Pessoa (RN-013), com o tipo (física/jurídica), a classificação
/// da Natureza Jurídica quando jurídica (RN-015) e o endereço principal quando existente.
/// </summary>
public sealed record PersonSearchItemDto(
    Guid Id,
    string DocumentNumber,
    string Name,
    string? SocialName,
    string Type,
    bool? IsPrivateSector,
    IReadOnlyList<string> Roles,
    PersonMainAddressDto? MainAddress);

public sealed record PersonMainAddressDto(
    string? ZipCode,
    string? Street,
    string? Number,
    string? Complement,
    string? Neighborhood,
    string? City,
    string? State);

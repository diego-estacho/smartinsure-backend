namespace SmartInsure.Core.Abstractions.Repositories.Dtos;

/// <summary>
/// Item de busca de Pessoa (RN-013), com o tipo (física/jurídica), a classificação
/// da Natureza Jurídica quando jurídica (RN-015), o endereço principal quando existente e a lista
/// completa de endereços (RN-503) — é dela que o corretor escolhe o endereço do Segurado da oferta.
/// <see cref="MainAddress"/> continua exposto para os consumidores que só precisam do principal.
/// </summary>
public sealed record PersonSearchItemDto(
    Guid Id,
    string DocumentNumber,
    string Name,
    string? SocialName,
    string Type,
    bool? IsPrivateSector,
    IReadOnlyList<string> Roles,
    PersonMainAddressDto? MainAddress,
    IReadOnlyList<PersonAddressItemDto> Addresses);

public sealed record PersonMainAddressDto(
    string? ZipCode,
    string? Street,
    string? Number,
    string? Complement,
    string? Neighborhood,
    string? City,
    string? State);

/// <summary>
/// Endereço da Pessoa identificado (RN-503): o Id é o que o corretor devolve ao escolher o endereço do
/// Segurado da oferta, e <see cref="IsMain"/> marca qual vem pré-selecionado.
/// </summary>
public sealed record PersonAddressItemDto(
    Guid Id,
    bool IsMain,
    string? ZipCode,
    string? Street,
    string? Number,
    string? Complement,
    string? Neighborhood,
    string? City,
    string? State);

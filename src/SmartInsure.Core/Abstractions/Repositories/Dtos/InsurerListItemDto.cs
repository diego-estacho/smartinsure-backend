namespace SmartInsure.Core.Abstractions.Repositories.Dtos;

/// <summary>Read-model da listagem do catálogo (RN-008) — status por nome estável.</summary>
public sealed record InsurerListItemDto(
    Guid Id,
    string Cnpj,
    string CorporateName,
    string? TradeName,
    string? LogoUrl,
    string Status);

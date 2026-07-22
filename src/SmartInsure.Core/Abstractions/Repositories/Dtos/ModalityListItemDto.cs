namespace SmartInsure.Core.Abstractions.Repositories.Dtos;

/// <summary>Read-model da listagem de Modalidades (RN-029) — status por nome estável.</summary>
public sealed record ModalityListItemDto(
    Guid Id,
    string Name,
    string? Description,
    string Status);

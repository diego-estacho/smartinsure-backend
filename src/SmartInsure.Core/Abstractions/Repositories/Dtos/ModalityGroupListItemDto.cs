namespace SmartInsure.Core.Abstractions.Repositories.Dtos;

/// <summary>Read-model da listagem de Grupos de Modalidade (RN-029) — status por nome estável.</summary>
public sealed record ModalityGroupListItemDto(
    Guid Id,
    string Name,
    string? Description,
    int DisplayOrder,
    string Status);

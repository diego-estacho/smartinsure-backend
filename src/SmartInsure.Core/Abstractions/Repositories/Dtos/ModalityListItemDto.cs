namespace SmartInsure.Core.Abstractions.Repositories.Dtos;

/// <summary>Read-model da listagem de Modalidades (RN-029) — status por nome estável, com o nome do Grupo.</summary>
public sealed record ModalityListItemDto(
    Guid Id,
    string Name,
    Guid ModalityGroupId,
    string ModalityGroupName,
    string? Description,
    string Status);

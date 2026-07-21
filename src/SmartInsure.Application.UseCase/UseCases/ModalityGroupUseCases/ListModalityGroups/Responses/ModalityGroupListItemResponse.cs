namespace SmartInsure.Application.UseCase.UseCases.ModalityGroupUseCases.ListModalityGroups.Responses;

/// <summary>Item de listagem do catálogo de Grupos de Modalidade (RN-029/RN-036).</summary>
public sealed record ModalityGroupListItemResponse(
    Guid Id,
    string Name,
    string? Description,
    int DisplayOrder,
    string Status);

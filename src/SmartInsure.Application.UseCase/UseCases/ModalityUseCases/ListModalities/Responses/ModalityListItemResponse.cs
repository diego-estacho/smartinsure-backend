namespace SmartInsure.Application.UseCase.UseCases.ModalityUseCases.ListModalities.Responses;

/// <summary>Item de listagem do catálogo de Modalidades (RN-029/RN-036), com o nome do Grupo.</summary>
public sealed record ModalityListItemResponse(
    Guid Id,
    string Name,
    Guid ModalityGroupId,
    string ModalityGroupName,
    string? Description,
    string Status);

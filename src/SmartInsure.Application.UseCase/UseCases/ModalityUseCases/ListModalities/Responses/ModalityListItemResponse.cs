namespace SmartInsure.Application.UseCase.UseCases.ModalityUseCases.ListModalities.Responses;

/// <summary>Item de listagem do catálogo de Modalidades (RN-029/RN-036).</summary>
public sealed record ModalityListItemResponse(
    Guid Id,
    string Name,
    string? Description,
    string Status);

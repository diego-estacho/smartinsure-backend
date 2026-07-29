namespace SmartInsure.Application.UseCase.UseCases.ModalityUseCases.UpdateModality.Responses;

/// <summary>Dados de saída da edição de Modalidade.</summary>
public sealed record UpdateModalityResponse(
    Guid Id,
    string Name,
    string? Description,
    string Status);

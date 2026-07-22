namespace SmartInsure.Application.UseCase.UseCases.ModalityUseCases.UpdateModality.Requests;

/// <summary>Dados de entrada para editar uma Modalidade (RN-029). O id vem da rota.</summary>
public sealed record UpdateModalityRequest(
    Guid ModalityId,
    string Name,
    string? Description);

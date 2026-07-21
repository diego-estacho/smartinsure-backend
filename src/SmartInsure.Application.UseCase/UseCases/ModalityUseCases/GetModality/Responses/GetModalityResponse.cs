namespace SmartInsure.Application.UseCase.UseCases.ModalityUseCases.GetModality.Responses;

/// <summary>Dados de detalhe de Modalidade do catálogo.</summary>
public sealed record GetModalityResponse(
    Guid Id,
    string Name,
    Guid ModalityGroupId,
    string? Description,
    string Status);

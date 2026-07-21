namespace SmartInsure.Application.UseCase.UseCases.ModalityUseCases.CreateModality.Responses;

/// <summary>Dados de saída do cadastro de Modalidade.</summary>
public sealed record CreateModalityResponse(
    Guid Id,
    string Name,
    Guid ModalityGroupId,
    string? Description,
    string Status);

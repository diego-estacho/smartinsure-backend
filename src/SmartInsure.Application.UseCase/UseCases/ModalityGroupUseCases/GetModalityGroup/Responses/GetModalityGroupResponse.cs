namespace SmartInsure.Application.UseCase.UseCases.ModalityGroupUseCases.GetModalityGroup.Responses;

/// <summary>Dados de detalhe de Grupo de Modalidade do catálogo.</summary>
public sealed record GetModalityGroupResponse(
    Guid Id,
    string Name,
    string? Description,
    int DisplayOrder,
    string Status);

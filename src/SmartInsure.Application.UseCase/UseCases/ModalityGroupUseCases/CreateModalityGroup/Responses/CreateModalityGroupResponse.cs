namespace SmartInsure.Application.UseCase.UseCases.ModalityGroupUseCases.CreateModalityGroup.Responses;

/// <summary>Dados de saída do cadastro de Grupo de Modalidade.</summary>
public sealed record CreateModalityGroupResponse(
    Guid Id,
    string Name,
    string? Description,
    int DisplayOrder,
    string Status);

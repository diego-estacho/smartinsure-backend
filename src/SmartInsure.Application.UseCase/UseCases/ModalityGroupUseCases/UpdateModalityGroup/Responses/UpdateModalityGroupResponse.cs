namespace SmartInsure.Application.UseCase.UseCases.ModalityGroupUseCases.UpdateModalityGroup.Responses;

/// <summary>Dados de saída da edição de Grupo de Modalidade.</summary>
public sealed record UpdateModalityGroupResponse(
    Guid Id,
    string Name,
    string? Description,
    int DisplayOrder,
    string Status);

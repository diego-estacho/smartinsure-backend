namespace SmartInsure.Application.UseCase.UseCases.ModalityGroupUseCases.UpdateModalityGroup.Requests;

/// <summary>Dados de entrada para editar um Grupo de Modalidade (RN-029). O id vem da rota.</summary>
public sealed record UpdateModalityGroupRequest(
    Guid ModalityGroupId,
    string Name,
    string? Description,
    int DisplayOrder);

namespace SmartInsure.Application.UseCase.UseCases.ModalityGroupUseCases.CreateModalityGroup.Requests;

/// <summary>Dados de entrada para cadastrar um Grupo de Modalidade (RN-029).</summary>
/// <param name="Name">Nome do grupo, único no catálogo.</param>
/// <param name="Description">Descrição do grupo (opcional).</param>
/// <param name="DisplayOrder">Ordem de exibição ao corretor.</param>
/// <param name="InitialStatus">Situação inicial pelo nome estável: Active ou Inactive.</param>
public sealed record CreateModalityGroupRequest(
    string Name,
    string? Description,
    int DisplayOrder,
    string InitialStatus);

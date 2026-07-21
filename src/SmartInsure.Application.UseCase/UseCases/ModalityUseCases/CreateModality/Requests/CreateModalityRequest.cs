namespace SmartInsure.Application.UseCase.UseCases.ModalityUseCases.CreateModality.Requests;

/// <summary>Dados de entrada para cadastrar uma Modalidade (RN-029).</summary>
/// <param name="Name">Nome de exibição, único no catálogo.</param>
/// <param name="ModalityGroupId">Grupo de Modalidade a que pertence.</param>
/// <param name="Description">Descrição em linguagem de negócio (opcional).</param>
/// <param name="InitialStatus">Situação inicial pelo nome estável: Active ou Inactive.</param>
public sealed record CreateModalityRequest(
    string Name,
    Guid ModalityGroupId,
    string? Description,
    string InitialStatus);

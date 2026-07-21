namespace SmartInsure.Application.UseCase.UseCases.ModalityGroupUseCases.ChangeModalityGroupStatus.Requests;

/// <summary>Dados de entrada para alterar a situação de um Grupo de Modalidade (RN-036). O id vem da rota.</summary>
public sealed record ChangeModalityGroupStatusRequest(
    Guid ModalityGroupId,
    string Status);

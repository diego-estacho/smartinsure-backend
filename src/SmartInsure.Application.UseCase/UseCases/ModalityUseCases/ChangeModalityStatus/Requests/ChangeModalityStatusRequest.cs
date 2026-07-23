namespace SmartInsure.Application.UseCase.UseCases.ModalityUseCases.ChangeModalityStatus.Requests;

/// <summary>Dados de entrada para alterar a situação de uma Modalidade (RN-039). O id vem da rota.</summary>
public sealed record ChangeModalityStatusRequest(
    Guid ModalityId,
    string Status);

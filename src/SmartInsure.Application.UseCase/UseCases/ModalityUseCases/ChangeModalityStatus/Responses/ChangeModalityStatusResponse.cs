namespace SmartInsure.Application.UseCase.UseCases.ModalityUseCases.ChangeModalityStatus.Responses;

/// <summary>Dados de saída da alteração de situação de Modalidade.</summary>
public sealed record ChangeModalityStatusResponse(
    Guid Id,
    string Status);

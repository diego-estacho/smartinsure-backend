namespace SmartInsure.Application.UseCase.UseCases.ModalityGroupUseCases.ChangeModalityGroupStatus.Responses;

/// <summary>Dados de saída da alteração de situação de Grupo de Modalidade.</summary>
public sealed record ChangeModalityGroupStatusResponse(
    Guid Id,
    string Status);

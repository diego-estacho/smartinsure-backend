namespace SmartInsure.Application.UseCase.UseCases.InsurerUseCases.ChangeInsurerStatus.Responses;

/// <summary>Dados de saída da alteração de situação de Seguradora.</summary>
public sealed record ChangeInsurerStatusResponse(
    Guid Id,
    string Status);

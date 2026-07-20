namespace SmartInsure.Application.UseCase.UseCases.CalculationEngineUseCases.ListCalculationEngines.Responses;

/// <summary>Motor de Cálculo disponível na plataforma (RN-023), pelo nome estável.</summary>
public sealed record CalculationEngineListItemResponse(string Name);

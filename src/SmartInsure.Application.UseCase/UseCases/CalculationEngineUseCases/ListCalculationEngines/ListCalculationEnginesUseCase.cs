using SmartInsure.Application.UseCase.UseCases.CalculationEngineUseCases.ListCalculationEngines.Interfaces;
using SmartInsure.Application.UseCase.UseCases.CalculationEngineUseCases.ListCalculationEngines.Requests;
using SmartInsure.Application.UseCase.UseCases.CalculationEngineUseCases.ListCalculationEngines.Responses;
using SmartInsure.Core.Enumerators;

namespace SmartInsure.Application.UseCase.UseCases.CalculationEngineUseCases.ListCalculationEngines;

/// <summary>RN-023 — Motores de Cálculo disponíveis na plataforma, pelo nome estável do catálogo de motores.</summary>
public sealed class ListCalculationEnginesUseCase : IListCalculationEnginesUseCase
{
    public Task<IReadOnlyList<CalculationEngineListItemResponse>> ExecuteAsync(
        ListCalculationEnginesRequest request,
        CancellationToken cancellationToken)
        => Task.FromResult<IReadOnlyList<CalculationEngineListItemResponse>>(
            [.. Enum.GetNames<ECalculationEngine>()
                .Select(name => new CalculationEngineListItemResponse(name))]);
}

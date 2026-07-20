using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.CalculationEngineUseCases.ListCalculationEngines.Requests;
using SmartInsure.Application.UseCase.UseCases.CalculationEngineUseCases.ListCalculationEngines.Responses;

namespace SmartInsure.Application.UseCase.UseCases.CalculationEngineUseCases.ListCalculationEngines.Interfaces;

public interface IListCalculationEnginesUseCase
    : IUseCase<ListCalculationEnginesRequest, IReadOnlyList<CalculationEngineListItemResponse>>;

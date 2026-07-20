using Carter;
using SmartInsure.Api.Handlers.Base;
using SmartInsure.Application.UseCase.UseCases.CalculationEngineUseCases.ListCalculationEngines.Interfaces;
using SmartInsure.Application.UseCase.UseCases.CalculationEngineUseCases.ListCalculationEngines.Requests;
using SmartInsure.Application.UseCase.UseCases.CalculationEngineUseCases.ListCalculationEngines.Responses;

namespace SmartInsure.Api.Endpoints;

/// <summary>Jornada Motor de Cálculo (RN-023): motores disponíveis para a Habilitação de Seguradora.</summary>
public sealed class CalculationEnginesEndpoint : CarterModule
{
    public CalculationEnginesEndpoint()
        : base("calculation-engines")
    {
    }

    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/", ListAsync)
            .Produces<IReadOnlyList<CalculationEngineListItemResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> ListAsync(
        HttpContext httpContext,
        RequestHandler handler,
        IListCalculationEnginesUseCase useCase)
        => await handler.TryHandleAsync(httpContext, useCase, new ListCalculationEnginesRequest());
}

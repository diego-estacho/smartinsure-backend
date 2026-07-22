using Carter;
using SmartInsure.Api.Handlers.Base;
using SmartInsure.Application.UseCase.UseCases.ModalityMapUseCases.GetModalityMap.Interfaces;
using SmartInsure.Application.UseCase.UseCases.ModalityMapUseCases.GetModalityMap.Requests;
using SmartInsure.Application.UseCase.UseCases.ModalityMapUseCases.GetModalityMap.Responses;
using SmartInsure.Core.Constants;

namespace SmartInsure.Api.Endpoints;

/// <summary>
/// Mapa de Modalidades (RN-033/RN-034): matriz Seguradoras × Modalidades com a Fila de pendências.
/// Restrito ao Administrador do Sistema (curadoria).
/// </summary>
public sealed class ModalityMapEndpoint : CarterModule
{
    public ModalityMapEndpoint()
        : base("modality-map")
    {
    }

    public override void AddRoutes(IEndpointRouteBuilder app)
        => app.MapGet("/", GetAsync)
            .RequireAuthorization(Policies.SystemAdministrator)
            .Produces<ModalityMapResponse>(StatusCodes.Status200OK);

    private static async Task<IResult> GetAsync(
        HttpContext httpContext,
        RequestHandler handler,
        IGetModalityMapUseCase useCase)
        => await handler.TryHandleAsync(httpContext, useCase, new GetModalityMapRequest());
}

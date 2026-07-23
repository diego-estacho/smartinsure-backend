using Carter;
using SmartInsure.Api.Handlers.Base;
using SmartInsure.Application.UseCase.UseCases.ModalityImportUseCases.RunModalityImport.Interfaces;
using SmartInsure.Application.UseCase.UseCases.ModalityImportUseCases.RunModalityImport.Requests;
using SmartInsure.Application.UseCase.UseCases.ModalityImportUseCases.RunModalityImport.Responses;
using SmartInsure.Core.Constants;

namespace SmartInsure.Api.Endpoints;

/// <summary>
/// Importação de modalidades (RN-034): disparo manual restrito ao Administrador do Sistema
/// (operação/teste). O agendado roda pelo timer das Functions (OPEN-10).
/// </summary>
public sealed class ModalityImportsEndpoint : CarterModule
{
    public ModalityImportsEndpoint()
        : base("modality-imports")
    {
    }

    public override void AddRoutes(IEndpointRouteBuilder app)
        => app.MapPost("/run", RunAsync)
            .RequireAuthorization(Policies.SystemAdministrator)
            .Produces<ModalityImportSummaryResponse>(StatusCodes.Status200OK);

    private static async Task<IResult> RunAsync(
        HttpContext httpContext,
        RequestHandler handler,
        IRunModalityImportUseCase useCase)
        => await handler.TryHandleAsync(httpContext, useCase, new RunModalityImportRequest());
}

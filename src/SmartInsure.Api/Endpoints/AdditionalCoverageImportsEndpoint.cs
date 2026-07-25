using Carter;
using SmartInsure.Api.Handlers.Base;
using SmartInsure.Application.UseCase.UseCases.AdditionalCoverageImportUseCases.RunAdditionalCoverageImport.Interfaces;
using SmartInsure.Application.UseCase.UseCases.AdditionalCoverageImportUseCases.RunAdditionalCoverageImport.Requests;
using SmartInsure.Application.UseCase.UseCases.AdditionalCoverageImportUseCases.RunAdditionalCoverageImport.Responses;
using SmartInsure.Core.Constants;

namespace SmartInsure.Api.Endpoints;

/// <summary>
/// Importação de Coberturas Adicionais (RN-044): disparo sob demanda restrito ao Administrador do
/// Sistema. O agendado roda pelo timer das Functions (cadência configurável, OPEN-10).
/// </summary>
public sealed class AdditionalCoverageImportsEndpoint : CarterModule
{
    public AdditionalCoverageImportsEndpoint()
        : base("additional-coverage-imports")
    {
    }

    public override void AddRoutes(IEndpointRouteBuilder app)
        => app.MapPost("/run", RunAsync)
            .RequireAuthorization(Policies.SystemAdministrator)
            .Produces<AdditionalCoverageImportSummaryResponse>(StatusCodes.Status200OK);

    private static async Task<IResult> RunAsync(
        HttpContext httpContext,
        RequestHandler handler,
        IRunAdditionalCoverageImportUseCase useCase)
        => await handler.TryHandleAsync(httpContext, useCase, new RunAdditionalCoverageImportRequest());
}

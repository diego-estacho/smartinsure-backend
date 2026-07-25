using Carter;
using SmartInsure.Api.Handlers.Base;
using SmartInsure.Application.UseCase.UseCases.ImportedAdditionalCoverageUseCases.IgnoreImportedAdditionalCoverage.Interfaces;
using SmartInsure.Application.UseCase.UseCases.ImportedAdditionalCoverageUseCases.IgnoreImportedAdditionalCoverage.Requests;
using SmartInsure.Application.UseCase.UseCases.ImportedAdditionalCoverageUseCases.IgnoreImportedAdditionalCoverage.Responses;
using SmartInsure.Application.UseCase.UseCases.ImportedAdditionalCoverageUseCases.LinkImportedAdditionalCoverage.Interfaces;
using SmartInsure.Application.UseCase.UseCases.ImportedAdditionalCoverageUseCases.LinkImportedAdditionalCoverage.Requests;
using SmartInsure.Application.UseCase.UseCases.ImportedAdditionalCoverageUseCases.LinkImportedAdditionalCoverage.Responses;
using SmartInsure.Application.UseCase.UseCases.ImportedAdditionalCoverageUseCases.RestoreImportedAdditionalCoverage.Interfaces;
using SmartInsure.Application.UseCase.UseCases.ImportedAdditionalCoverageUseCases.RestoreImportedAdditionalCoverage.Requests;
using SmartInsure.Application.UseCase.UseCases.ImportedAdditionalCoverageUseCases.RestoreImportedAdditionalCoverage.Responses;
using SmartInsure.Application.UseCase.UseCases.ImportedAdditionalCoverageUseCases.UnlinkImportedAdditionalCoverage.Interfaces;
using SmartInsure.Application.UseCase.UseCases.ImportedAdditionalCoverageUseCases.UnlinkImportedAdditionalCoverage.Requests;
using SmartInsure.Application.UseCase.UseCases.ImportedAdditionalCoverageUseCases.UnlinkImportedAdditionalCoverage.Responses;
using SmartInsure.Core.Constants;

namespace SmartInsure.Api.Endpoints;

/// <summary>
/// Curadoria da Cobertura Adicional Importada (RN-043): o Administrador do Sistema vincula/reatribui
/// a importada a uma Cobertura Adicional canônica, desfaz o vínculo, ignora ou reativa uma pendência.
/// Escrita restrita ao Administrador do Sistema.
/// </summary>
public sealed class ImportedAdditionalCoveragesEndpoint : CarterModule
{
    public ImportedAdditionalCoveragesEndpoint()
        : base("imported-additional-coverages")
    {
    }

    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/{id:guid}/link", LinkAsync)
            .RequireAuthorization(Policies.SystemAdministrator)
            .Produces<LinkImportedAdditionalCoverageResponse>(StatusCodes.Status200OK);

        app.MapPost("/{id:guid}/unlink", UnlinkAsync)
            .RequireAuthorization(Policies.SystemAdministrator)
            .Produces<UnlinkImportedAdditionalCoverageResponse>(StatusCodes.Status200OK);

        app.MapPost("/{id:guid}/ignore", IgnoreAsync)
            .RequireAuthorization(Policies.SystemAdministrator)
            .Produces<IgnoreImportedAdditionalCoverageResponse>(StatusCodes.Status200OK);

        app.MapPost("/{id:guid}/restore", RestoreAsync)
            .RequireAuthorization(Policies.SystemAdministrator)
            .Produces<RestoreImportedAdditionalCoverageResponse>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> LinkAsync(
        HttpContext httpContext,
        RequestHandler handler,
        ILinkImportedAdditionalCoverageUseCase useCase,
        Guid id,
        LinkImportedAdditionalCoverageBody body)
        => await handler.TryHandleAsync(
            httpContext, useCase, new LinkImportedAdditionalCoverageRequest(id, body.AdditionalCoverageId));

    private static async Task<IResult> UnlinkAsync(
        HttpContext httpContext, RequestHandler handler, IUnlinkImportedAdditionalCoverageUseCase useCase, Guid id)
        => await handler.TryHandleAsync(httpContext, useCase, new UnlinkImportedAdditionalCoverageRequest(id));

    private static async Task<IResult> IgnoreAsync(
        HttpContext httpContext, RequestHandler handler, IIgnoreImportedAdditionalCoverageUseCase useCase, Guid id)
        => await handler.TryHandleAsync(httpContext, useCase, new IgnoreImportedAdditionalCoverageRequest(id));

    private static async Task<IResult> RestoreAsync(
        HttpContext httpContext, RequestHandler handler, IRestoreImportedAdditionalCoverageUseCase useCase, Guid id)
        => await handler.TryHandleAsync(httpContext, useCase, new RestoreImportedAdditionalCoverageRequest(id));
}

/// <summary>Corpo do vincular — a Cobertura Adicional canônica alvo; o id da Importada vem da rota.</summary>
public sealed record LinkImportedAdditionalCoverageBody(Guid AdditionalCoverageId);

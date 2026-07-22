using Carter;
using FluentValidation;
using SmartInsure.Api.Handlers.Base;
using SmartInsure.Application.UseCase.UseCases.ImportedModalityUseCases.IgnoreImportedModality.Interfaces;
using SmartInsure.Application.UseCase.UseCases.ImportedModalityUseCases.IgnoreImportedModality.Requests;
using SmartInsure.Application.UseCase.UseCases.ImportedModalityUseCases.IgnoreImportedModality.Responses;
using SmartInsure.Application.UseCase.UseCases.ImportedModalityUseCases.ReassignImportedModality.Interfaces;
using SmartInsure.Application.UseCase.UseCases.ImportedModalityUseCases.ReassignImportedModality.Requests;
using SmartInsure.Application.UseCase.UseCases.ImportedModalityUseCases.ReassignImportedModality.Responses;
using SmartInsure.Application.UseCase.UseCases.ImportedModalityUseCases.RestoreImportedModality.Interfaces;
using SmartInsure.Application.UseCase.UseCases.ImportedModalityUseCases.RestoreImportedModality.Requests;
using SmartInsure.Application.UseCase.UseCases.ImportedModalityUseCases.RestoreImportedModality.Responses;
using SmartInsure.Core.Constants;

namespace SmartInsure.Api.Endpoints;

/// <summary>
/// Curadoria da Fila de Revisão (RN-034): reatribuir manualmente uma Modalidade Importada a uma
/// Modalidade (override Manual), ignorá-la ou reativá-la. Escrita restrita ao Administrador do Sistema.
/// </summary>
public sealed class ImportedModalitiesEndpoint : CarterModule
{
    public ImportedModalitiesEndpoint()
        : base("imported-modalities")
    {
    }

    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/{id:guid}/reassign", ReassignAsync)
            .RequireAuthorization(Policies.SystemAdministrator)
            .Produces<ReassignImportedModalityResponse>(StatusCodes.Status200OK);

        app.MapPost("/{id:guid}/ignore", IgnoreAsync)
            .RequireAuthorization(Policies.SystemAdministrator)
            .Produces<IgnoreImportedModalityResponse>(StatusCodes.Status200OK);

        app.MapPost("/{id:guid}/restore", RestoreAsync)
            .RequireAuthorization(Policies.SystemAdministrator)
            .Produces<RestoreImportedModalityResponse>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> ReassignAsync(
        HttpContext httpContext,
        RequestHandler handler,
        IReassignImportedModalityUseCase useCase,
        IValidator<ReassignImportedModalityRequest> validator,
        Guid id,
        ReassignImportedModalityBody body)
        => await handler.TryHandleAsync(
            httpContext,
            useCase,
            new ReassignImportedModalityRequest(id, body.ModalityId),
            validator);

    private static async Task<IResult> IgnoreAsync(
        HttpContext httpContext,
        RequestHandler handler,
        IIgnoreImportedModalityUseCase useCase,
        Guid id)
        => await handler.TryHandleAsync(httpContext, useCase, new IgnoreImportedModalityRequest(id));

    private static async Task<IResult> RestoreAsync(
        HttpContext httpContext,
        RequestHandler handler,
        IRestoreImportedModalityUseCase useCase,
        Guid id)
        => await handler.TryHandleAsync(httpContext, useCase, new RestoreImportedModalityRequest(id));
}

/// <summary>Corpo do reatribuir — o id da Modalidade Importada vem da rota.</summary>
public sealed record ReassignImportedModalityBody(Guid ModalityId);

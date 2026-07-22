using Carter;
using FluentValidation;
using SmartInsure.Api.Handlers.Base;
using SmartInsure.Application.UseCase.UseCases.ImportedModalityUseCases.IgnoreImportedModality.Interfaces;
using SmartInsure.Application.UseCase.UseCases.ImportedModalityUseCases.IgnoreImportedModality.Requests;
using SmartInsure.Application.UseCase.UseCases.ImportedModalityUseCases.IgnoreImportedModality.Responses;
using SmartInsure.Application.UseCase.UseCases.ImportedModalityUseCases.MapImportedModality.Interfaces;
using SmartInsure.Application.UseCase.UseCases.ImportedModalityUseCases.MapImportedModality.Requests;
using SmartInsure.Application.UseCase.UseCases.ImportedModalityUseCases.MapImportedModality.Responses;
using SmartInsure.Core.Constants;

namespace SmartInsure.Api.Endpoints;

/// <summary>
/// Resolução da Fila de Revisão (RN-034): mapear uma Modalidade Importada pendente para uma
/// Modalidade, ou ignorá-la. Escrita restrita ao Administrador do Sistema.
/// </summary>
public sealed class ImportedModalitiesEndpoint : CarterModule
{
    public ImportedModalitiesEndpoint()
        : base("imported-modalities")
    {
    }

    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/{id:guid}/map", MapAsync)
            .RequireAuthorization(Policies.SystemAdministrator)
            .Produces<MapImportedModalityResponse>(StatusCodes.Status200OK);

        app.MapPost("/{id:guid}/ignore", IgnoreAsync)
            .RequireAuthorization(Policies.SystemAdministrator)
            .Produces<IgnoreImportedModalityResponse>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> MapAsync(
        HttpContext httpContext,
        RequestHandler handler,
        IMapImportedModalityUseCase useCase,
        IValidator<MapImportedModalityRequest> validator,
        Guid id,
        MapImportedModalityBody body)
        => await handler.TryHandleAsync(
            httpContext,
            useCase,
            new MapImportedModalityRequest(id, body.ModalityId),
            validator);

    private static async Task<IResult> IgnoreAsync(
        HttpContext httpContext,
        RequestHandler handler,
        IIgnoreImportedModalityUseCase useCase,
        Guid id)
        => await handler.TryHandleAsync(httpContext, useCase, new IgnoreImportedModalityRequest(id));
}

/// <summary>Corpo do mapear — o id da Modalidade Importada vem da rota.</summary>
public sealed record MapImportedModalityBody(Guid ModalityId);

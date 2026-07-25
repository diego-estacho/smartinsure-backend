using Carter;
using SmartInsure.Api.Handlers.Base;
using SmartInsure.Application.UseCase.UseCases.AdditionalCoverageUseCases.ActivateAdditionalCoverage.Interfaces;
using SmartInsure.Application.UseCase.UseCases.AdditionalCoverageUseCases.ActivateAdditionalCoverage.Requests;
using SmartInsure.Application.UseCase.UseCases.AdditionalCoverageUseCases.ActivateAdditionalCoverage.Responses;
using SmartInsure.Application.UseCase.UseCases.AdditionalCoverageUseCases.CreateAdditionalCoverage.Interfaces;
using SmartInsure.Application.UseCase.UseCases.AdditionalCoverageUseCases.CreateAdditionalCoverage.Requests;
using SmartInsure.Application.UseCase.UseCases.AdditionalCoverageUseCases.CreateAdditionalCoverage.Responses;
using SmartInsure.Application.UseCase.UseCases.AdditionalCoverageUseCases.GetAdditionalCoverageMap.Interfaces;
using SmartInsure.Application.UseCase.UseCases.AdditionalCoverageUseCases.GetAdditionalCoverageMap.Requests;
using SmartInsure.Application.UseCase.UseCases.AdditionalCoverageUseCases.GetAdditionalCoverageMap.Responses;
using SmartInsure.Application.UseCase.UseCases.AdditionalCoverageUseCases.InactivateAdditionalCoverage.Interfaces;
using SmartInsure.Application.UseCase.UseCases.AdditionalCoverageUseCases.InactivateAdditionalCoverage.Requests;
using SmartInsure.Application.UseCase.UseCases.AdditionalCoverageUseCases.InactivateAdditionalCoverage.Responses;
using SmartInsure.Application.UseCase.UseCases.AdditionalCoverageUseCases.UpdateAdditionalCoverage.Interfaces;
using SmartInsure.Application.UseCase.UseCases.AdditionalCoverageUseCases.UpdateAdditionalCoverage.Requests;
using SmartInsure.Application.UseCase.UseCases.AdditionalCoverageUseCases.UpdateAdditionalCoverage.Responses;
using SmartInsure.Core.Constants;

namespace SmartInsure.Api.Endpoints;

/// <summary>
/// Curadoria da Cobertura Adicional canônica (RN-040/RN-046): o Administrador do Sistema cria, edita,
/// ativa/inativa e consulta o Mapa (catálogo + Fila de pendências). Escrita restrita ao Administrador.
/// </summary>
public sealed class AdditionalCoveragesEndpoint : CarterModule
{
    public AdditionalCoveragesEndpoint()
        : base("additional-coverages")
    {
    }

    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/map", GetMapAsync)
            .RequireAuthorization(Policies.SystemAdministrator)
            .Produces<AdditionalCoverageMapResponse>(StatusCodes.Status200OK);

        app.MapPost("/", CreateAsync)
            .RequireAuthorization(Policies.SystemAdministrator)
            .Produces<CreateAdditionalCoverageResponse>(StatusCodes.Status200OK);

        app.MapPut("/{id:guid}", UpdateAsync)
            .RequireAuthorization(Policies.SystemAdministrator)
            .Produces<UpdateAdditionalCoverageResponse>(StatusCodes.Status200OK);

        app.MapPost("/{id:guid}/activate", ActivateAsync)
            .RequireAuthorization(Policies.SystemAdministrator)
            .Produces<ActivateAdditionalCoverageResponse>(StatusCodes.Status200OK);

        app.MapPost("/{id:guid}/inactivate", InactivateAsync)
            .RequireAuthorization(Policies.SystemAdministrator)
            .Produces<InactivateAdditionalCoverageResponse>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> GetMapAsync(
        HttpContext httpContext, RequestHandler handler, IGetAdditionalCoverageMapUseCase useCase)
        => await handler.TryHandleAsync(httpContext, useCase, new GetAdditionalCoverageMapRequest());

    private static async Task<IResult> CreateAsync(
        HttpContext httpContext,
        RequestHandler handler,
        ICreateAdditionalCoverageUseCase useCase,
        AdditionalCoverageNameBody body)
        => await handler.TryHandleAsync(httpContext, useCase, new CreateAdditionalCoverageRequest(body.Name));

    private static async Task<IResult> UpdateAsync(
        HttpContext httpContext,
        RequestHandler handler,
        IUpdateAdditionalCoverageUseCase useCase,
        Guid id,
        AdditionalCoverageNameBody body)
        => await handler.TryHandleAsync(httpContext, useCase, new UpdateAdditionalCoverageRequest(id, body.Name));

    private static async Task<IResult> ActivateAsync(
        HttpContext httpContext, RequestHandler handler, IActivateAdditionalCoverageUseCase useCase, Guid id)
        => await handler.TryHandleAsync(httpContext, useCase, new ActivateAdditionalCoverageRequest(id));

    private static async Task<IResult> InactivateAsync(
        HttpContext httpContext, RequestHandler handler, IInactivateAdditionalCoverageUseCase useCase, Guid id)
        => await handler.TryHandleAsync(httpContext, useCase, new InactivateAdditionalCoverageRequest(id));
}

/// <summary>Corpo com o nome da Cobertura Adicional canônica (criar/editar).</summary>
public sealed record AdditionalCoverageNameBody(string Name);

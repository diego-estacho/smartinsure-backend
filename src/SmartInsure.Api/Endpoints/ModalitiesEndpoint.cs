using Carter;
using FluentValidation;
using SmartInsure.Api.Handlers.Base;
using SmartInsure.Application.UseCase.ModelsBase;
using SmartInsure.Application.UseCase.UseCases.ModalityUseCases.ChangeModalityStatus.Interfaces;
using SmartInsure.Application.UseCase.UseCases.ModalityUseCases.ChangeModalityStatus.Requests;
using SmartInsure.Application.UseCase.UseCases.ModalityUseCases.ChangeModalityStatus.Responses;
using SmartInsure.Application.UseCase.UseCases.ModalityUseCases.CreateModality.Interfaces;
using SmartInsure.Application.UseCase.UseCases.ModalityUseCases.CreateModality.Requests;
using SmartInsure.Application.UseCase.UseCases.ModalityUseCases.CreateModality.Responses;
using SmartInsure.Application.UseCase.UseCases.ModalityUseCases.GetModality.Interfaces;
using SmartInsure.Application.UseCase.UseCases.ModalityUseCases.GetModality.Requests;
using SmartInsure.Application.UseCase.UseCases.ModalityUseCases.GetModality.Responses;
using SmartInsure.Application.UseCase.UseCases.ModalityUseCases.ListModalities.Interfaces;
using SmartInsure.Application.UseCase.UseCases.ModalityUseCases.ListModalities.Requests;
using SmartInsure.Application.UseCase.UseCases.ModalityUseCases.ListModalities.Responses;
using SmartInsure.Application.UseCase.UseCases.ModalityUseCases.UpdateModality.Interfaces;
using SmartInsure.Application.UseCase.UseCases.ModalityUseCases.UpdateModality.Requests;
using SmartInsure.Application.UseCase.UseCases.ModalityUseCases.UpdateModality.Responses;
using SmartInsure.Core.Constants;

namespace SmartInsure.Api.Endpoints;

/// <summary>
/// Catálogo de Modalidades (RN-029/RN-036): escrita exclusiva do Administrador do Sistema
/// (policy fail-closed); leitura para qualquer autenticado. Nunca há exclusão — só inativação.
/// </summary>
public sealed class ModalitiesEndpoint : CarterModule
{
    public ModalitiesEndpoint()
        : base("modalities")
    {
    }

    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/", CreateAsync)
            .RequireAuthorization(Policies.SystemAdministrator)
            .Produces<CreateModalityResponse>(StatusCodes.Status201Created);

        app.MapPut("/{id:guid}", UpdateAsync)
            .RequireAuthorization(Policies.SystemAdministrator)
            .Produces<UpdateModalityResponse>(StatusCodes.Status200OK);

        app.MapPatch("/{id:guid}/status", ChangeStatusAsync)
            .RequireAuthorization(Policies.SystemAdministrator)
            .Produces<ChangeModalityStatusResponse>(StatusCodes.Status200OK);

        app.MapGet("/", ListAsync)
            .Produces<PagedResponse<ModalityListItemResponse>>(StatusCodes.Status200OK);

        app.MapGet("/{id:guid}", GetAsync)
            .Produces<GetModalityResponse>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> CreateAsync(
        HttpContext httpContext,
        RequestHandler handler,
        ICreateModalityUseCase useCase,
        IValidator<CreateModalityRequest> validator,
        CreateModalityRequest request)
        => await handler.TryHandleAsync(
            httpContext,
            useCase,
            request,
            validator,
            response => Results.Created($"/api/v1/modalities/{response.Id}", response));

    private static async Task<IResult> UpdateAsync(
        HttpContext httpContext,
        RequestHandler handler,
        IUpdateModalityUseCase useCase,
        IValidator<UpdateModalityRequest> validator,
        Guid id,
        UpdateModalityBody body)
        => await handler.TryHandleAsync(
            httpContext,
            useCase,
            new UpdateModalityRequest(id, body.Name, body.ModalityGroupId, body.Description),
            validator);

    private static async Task<IResult> ChangeStatusAsync(
        HttpContext httpContext,
        RequestHandler handler,
        IChangeModalityStatusUseCase useCase,
        IValidator<ChangeModalityStatusRequest> validator,
        Guid id,
        ChangeModalityStatusBody body)
        => await handler.TryHandleAsync(
            httpContext,
            useCase,
            new ChangeModalityStatusRequest(id, body.Status),
            validator);

    /// <summary>RN-033/RN-036: visão completa só surte efeito para o Administrador do Sistema.</summary>
    private static async Task<IResult> ListAsync(
        HttpContext httpContext,
        RequestHandler handler,
        IListModalitiesUseCase useCase,
        int? page,
        int? pageSize,
        bool? includeInactive)
        => await handler.TryHandleAsync(
            httpContext,
            useCase,
            new ListModalitiesRequest
            {
                Page = page ?? 1,
                PageSize = pageSize ?? 20,
                IncludeInactive = includeInactive ?? false,
                CallerIsSystemAdministrator = httpContext.User.IsInRole(Roles.SystemAdministrator),
            });

    private static async Task<IResult> GetAsync(
        HttpContext httpContext,
        RequestHandler handler,
        IGetModalityUseCase useCase,
        Guid id)
        => await handler.TryHandleAsync(httpContext, useCase, new GetModalityRequest(id));
}

/// <summary>Corpo do PUT de Modalidade — o id vem da rota.</summary>
public sealed record UpdateModalityBody(string Name, Guid ModalityGroupId, string? Description);

/// <summary>Corpo do PATCH de situação de Modalidade — nome estável Active/Inactive.</summary>
public sealed record ChangeModalityStatusBody(string Status);

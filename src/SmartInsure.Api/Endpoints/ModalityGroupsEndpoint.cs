using Carter;
using FluentValidation;
using SmartInsure.Api.Handlers.Base;
using SmartInsure.Application.UseCase.ModelsBase;
using SmartInsure.Application.UseCase.UseCases.ModalityGroupUseCases.ChangeModalityGroupStatus.Interfaces;
using SmartInsure.Application.UseCase.UseCases.ModalityGroupUseCases.ChangeModalityGroupStatus.Requests;
using SmartInsure.Application.UseCase.UseCases.ModalityGroupUseCases.ChangeModalityGroupStatus.Responses;
using SmartInsure.Application.UseCase.UseCases.ModalityGroupUseCases.CreateModalityGroup.Interfaces;
using SmartInsure.Application.UseCase.UseCases.ModalityGroupUseCases.CreateModalityGroup.Requests;
using SmartInsure.Application.UseCase.UseCases.ModalityGroupUseCases.CreateModalityGroup.Responses;
using SmartInsure.Application.UseCase.UseCases.ModalityGroupUseCases.GetModalityGroup.Interfaces;
using SmartInsure.Application.UseCase.UseCases.ModalityGroupUseCases.GetModalityGroup.Requests;
using SmartInsure.Application.UseCase.UseCases.ModalityGroupUseCases.GetModalityGroup.Responses;
using SmartInsure.Application.UseCase.UseCases.ModalityGroupUseCases.ListModalityGroups.Interfaces;
using SmartInsure.Application.UseCase.UseCases.ModalityGroupUseCases.ListModalityGroups.Requests;
using SmartInsure.Application.UseCase.UseCases.ModalityGroupUseCases.ListModalityGroups.Responses;
using SmartInsure.Application.UseCase.UseCases.ModalityGroupUseCases.UpdateModalityGroup.Interfaces;
using SmartInsure.Application.UseCase.UseCases.ModalityGroupUseCases.UpdateModalityGroup.Requests;
using SmartInsure.Application.UseCase.UseCases.ModalityGroupUseCases.UpdateModalityGroup.Responses;
using SmartInsure.Core.Constants;

namespace SmartInsure.Api.Endpoints;

/// <summary>
/// Catálogo de Grupos de Modalidade (RN-029/RN-036): escrita exclusiva do Administrador do Sistema
/// (policy fail-closed); leitura para qualquer autenticado. Nunca há exclusão — só inativação.
/// </summary>
public sealed class ModalityGroupsEndpoint : CarterModule
{
    public ModalityGroupsEndpoint()
        : base("modality-groups")
    {
    }

    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/", CreateAsync)
            .RequireAuthorization(Policies.SystemAdministrator)
            .Produces<CreateModalityGroupResponse>(StatusCodes.Status201Created);

        app.MapPut("/{id:guid}", UpdateAsync)
            .RequireAuthorization(Policies.SystemAdministrator)
            .Produces<UpdateModalityGroupResponse>(StatusCodes.Status200OK);

        app.MapPatch("/{id:guid}/status", ChangeStatusAsync)
            .RequireAuthorization(Policies.SystemAdministrator)
            .Produces<ChangeModalityGroupStatusResponse>(StatusCodes.Status200OK);

        app.MapGet("/", ListAsync)
            .Produces<PagedResponse<ModalityGroupListItemResponse>>(StatusCodes.Status200OK);

        app.MapGet("/{id:guid}", GetAsync)
            .Produces<GetModalityGroupResponse>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> CreateAsync(
        HttpContext httpContext,
        RequestHandler handler,
        ICreateModalityGroupUseCase useCase,
        IValidator<CreateModalityGroupRequest> validator,
        CreateModalityGroupRequest request)
        => await handler.TryHandleAsync(
            httpContext,
            useCase,
            request,
            validator,
            response => Results.Created($"/api/v1/modality-groups/{response.Id}", response));

    private static async Task<IResult> UpdateAsync(
        HttpContext httpContext,
        RequestHandler handler,
        IUpdateModalityGroupUseCase useCase,
        IValidator<UpdateModalityGroupRequest> validator,
        Guid id,
        UpdateModalityGroupBody body)
        => await handler.TryHandleAsync(
            httpContext,
            useCase,
            new UpdateModalityGroupRequest(id, body.Name, body.Description, body.DisplayOrder),
            validator);

    private static async Task<IResult> ChangeStatusAsync(
        HttpContext httpContext,
        RequestHandler handler,
        IChangeModalityGroupStatusUseCase useCase,
        IValidator<ChangeModalityGroupStatusRequest> validator,
        Guid id,
        ChangeModalityGroupStatusBody body)
        => await handler.TryHandleAsync(
            httpContext,
            useCase,
            new ChangeModalityGroupStatusRequest(id, body.Status),
            validator);

    /// <summary>RN-036: visão completa só surte efeito para o Administrador do Sistema.</summary>
    private static async Task<IResult> ListAsync(
        HttpContext httpContext,
        RequestHandler handler,
        IListModalityGroupsUseCase useCase,
        int? page,
        int? pageSize,
        bool? includeInactive)
        => await handler.TryHandleAsync(
            httpContext,
            useCase,
            new ListModalityGroupsRequest
            {
                Page = page ?? 1,
                PageSize = pageSize ?? 20,
                IncludeInactive = includeInactive ?? false,
                CallerIsSystemAdministrator = httpContext.User.IsInRole(Roles.SystemAdministrator),
            });

    private static async Task<IResult> GetAsync(
        HttpContext httpContext,
        RequestHandler handler,
        IGetModalityGroupUseCase useCase,
        Guid id)
        => await handler.TryHandleAsync(httpContext, useCase, new GetModalityGroupRequest(id));
}

/// <summary>Corpo do PUT de Grupo — o id vem da rota.</summary>
public sealed record UpdateModalityGroupBody(string Name, string? Description, int DisplayOrder);

/// <summary>Corpo do PATCH de situação de Grupo — nome estável Active/Inactive.</summary>
public sealed record ChangeModalityGroupStatusBody(string Status);

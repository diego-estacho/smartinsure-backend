using Carter;
using FluentValidation;
using SmartInsure.Api.Handlers.Base;
using SmartInsure.Application.UseCase.ModelsBase;
using SmartInsure.Application.UseCase.UseCases.InsurerEnablementUseCases.ChangeInsurerEnablementStatus.Interfaces;
using SmartInsure.Application.UseCase.UseCases.InsurerEnablementUseCases.ChangeInsurerEnablementStatus.Requests;
using SmartInsure.Application.UseCase.UseCases.InsurerEnablementUseCases.ChangeInsurerEnablementStatus.Responses;
using SmartInsure.Application.UseCase.UseCases.InsurerEnablementUseCases.CreateInsurerEnablement.Interfaces;
using SmartInsure.Application.UseCase.UseCases.InsurerEnablementUseCases.CreateInsurerEnablement.Requests;
using SmartInsure.Application.UseCase.UseCases.InsurerEnablementUseCases.CreateInsurerEnablement.Responses;
using SmartInsure.Application.UseCase.UseCases.InsurerEnablementUseCases.GetInsurerEnablement.Interfaces;
using SmartInsure.Application.UseCase.UseCases.InsurerEnablementUseCases.GetInsurerEnablement.Requests;
using SmartInsure.Application.UseCase.UseCases.InsurerEnablementUseCases.GetInsurerEnablement.Responses;
using SmartInsure.Application.UseCase.UseCases.InsurerEnablementUseCases.ListInsurerEnablements.Interfaces;
using SmartInsure.Application.UseCase.UseCases.InsurerEnablementUseCases.ListInsurerEnablements.Requests;
using SmartInsure.Application.UseCase.UseCases.InsurerEnablementUseCases.ListInsurerEnablements.Responses;
using SmartInsure.Application.UseCase.UseCases.InsurerEnablementUseCases.UpdateInsurerEnablement.Interfaces;
using SmartInsure.Application.UseCase.UseCases.InsurerEnablementUseCases.UpdateInsurerEnablement.Requests;
using SmartInsure.Application.UseCase.UseCases.InsurerEnablementUseCases.UpdateInsurerEnablement.Responses;

namespace SmartInsure.Api.Endpoints;

/// <summary>
/// Jornada Motor de Cálculo (RN-022..RN-023): Habilitação de Seguradora do par
/// Corretora×Seguradora. Nesta fase qualquer autenticado gerencia (OPEN-07/OPEN-03).
/// </summary>
public sealed class InsurerEnablementsEndpoint : CarterModule
{
    public InsurerEnablementsEndpoint()
        : base("insurer-enablements")
    {
    }

    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/", CreateAsync)
            .Produces<CreateInsurerEnablementResponse>(StatusCodes.Status201Created);

        app.MapPut("/{id:guid}", UpdateAsync)
            .Produces<UpdateInsurerEnablementResponse>(StatusCodes.Status200OK);

        app.MapPatch("/{id:guid}/status", ChangeStatusAsync)
            .Produces<ChangeInsurerEnablementStatusResponse>(StatusCodes.Status200OK);

        app.MapGet("/", ListAsync)
            .Produces<PagedResponse<InsurerEnablementListItemResponse>>(StatusCodes.Status200OK);

        app.MapGet("/{id:guid}", GetAsync)
            .Produces<GetInsurerEnablementResponse>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> CreateAsync(
        HttpContext httpContext,
        RequestHandler handler,
        ICreateInsurerEnablementUseCase useCase,
        IValidator<CreateInsurerEnablementRequest> validator,
        CreateInsurerEnablementRequest request)
        => await handler.TryHandleAsync(
            httpContext,
            useCase,
            request,
            validator,
            response => Results.Created($"/api/v1/insurer-enablements/{response.Id}", response));

    private static async Task<IResult> UpdateAsync(
        HttpContext httpContext,
        RequestHandler handler,
        IUpdateInsurerEnablementUseCase useCase,
        IValidator<UpdateInsurerEnablementRequest> validator,
        Guid id,
        UpdateInsurerEnablementBody body)
        => await handler.TryHandleAsync(
            httpContext,
            useCase,
            new UpdateInsurerEnablementRequest(id, body.CalculationEngine, body.ConnectionParameters),
            validator);

    private static async Task<IResult> ChangeStatusAsync(
        HttpContext httpContext,
        RequestHandler handler,
        IChangeInsurerEnablementStatusUseCase useCase,
        IValidator<ChangeInsurerEnablementStatusRequest> validator,
        Guid id,
        ChangeInsurerEnablementStatusBody body)
        => await handler.TryHandleAsync(
            httpContext,
            useCase,
            new ChangeInsurerEnablementStatusRequest(id, body.Status),
            validator);

    private static async Task<IResult> ListAsync(
        HttpContext httpContext,
        RequestHandler handler,
        IListInsurerEnablementsUseCase useCase,
        Guid? brokerageId,
        int? page,
        int? pageSize)
        => await handler.TryHandleAsync(
            httpContext,
            useCase,
            new ListInsurerEnablementsRequest
            {
                BrokerageId = brokerageId,
                Page = page ?? 1,
                PageSize = pageSize ?? 20,
            });

    private static async Task<IResult> GetAsync(
        HttpContext httpContext,
        RequestHandler handler,
        IGetInsurerEnablementUseCase useCase,
        Guid id)
        => await handler.TryHandleAsync(httpContext, useCase, new GetInsurerEnablementRequest(id));
}

/// <summary>Corpo do PUT — o id vem da rota.</summary>
public sealed record UpdateInsurerEnablementBody(
    string CalculationEngine, string? ConnectionParameters);

/// <summary>Corpo do PATCH de situação — nome estável Active/Inactive.</summary>
public sealed record ChangeInsurerEnablementStatusBody(string Status);

using Carter;
using FluentValidation;
using SmartInsure.Api.Handlers.Base;
using SmartInsure.Application.UseCase.ModelsBase;
using SmartInsure.Application.UseCase.UseCases.BrokerageInsurerEnablementUseCases.ChangeBrokerageInsurerEnablementStatus.Interfaces;
using SmartInsure.Application.UseCase.UseCases.BrokerageInsurerEnablementUseCases.ChangeBrokerageInsurerEnablementStatus.Requests;
using SmartInsure.Application.UseCase.UseCases.BrokerageInsurerEnablementUseCases.ChangeBrokerageInsurerEnablementStatus.Responses;
using SmartInsure.Application.UseCase.UseCases.BrokerageInsurerEnablementUseCases.CreateBrokerageInsurerEnablement.Interfaces;
using SmartInsure.Application.UseCase.UseCases.BrokerageInsurerEnablementUseCases.CreateBrokerageInsurerEnablement.Requests;
using SmartInsure.Application.UseCase.UseCases.BrokerageInsurerEnablementUseCases.CreateBrokerageInsurerEnablement.Responses;
using SmartInsure.Application.UseCase.UseCases.BrokerageInsurerEnablementUseCases.GetBrokerageInsurerEnablement.Interfaces;
using SmartInsure.Application.UseCase.UseCases.BrokerageInsurerEnablementUseCases.GetBrokerageInsurerEnablement.Requests;
using SmartInsure.Application.UseCase.UseCases.BrokerageInsurerEnablementUseCases.GetBrokerageInsurerEnablement.Responses;
using SmartInsure.Application.UseCase.UseCases.BrokerageInsurerEnablementUseCases.ListBrokerageInsurerEnablements.Interfaces;
using SmartInsure.Application.UseCase.UseCases.BrokerageInsurerEnablementUseCases.ListBrokerageInsurerEnablements.Requests;
using SmartInsure.Application.UseCase.UseCases.BrokerageInsurerEnablementUseCases.ListBrokerageInsurerEnablements.Responses;
using SmartInsure.Application.UseCase.UseCases.BrokerageInsurerEnablementUseCases.UpdateBrokerageInsurerEnablement.Interfaces;
using SmartInsure.Application.UseCase.UseCases.BrokerageInsurerEnablementUseCases.UpdateBrokerageInsurerEnablement.Requests;
using SmartInsure.Application.UseCase.UseCases.BrokerageInsurerEnablementUseCases.UpdateBrokerageInsurerEnablement.Responses;

namespace SmartInsure.Api.Endpoints;

/// <summary>
/// Jornada Motor de Cálculo (RN-022..RN-023): Habilitação de Seguradora do par
/// Corretora×Seguradora. Nesta fase qualquer autenticado gerencia (OPEN-07/OPEN-03).
/// </summary>
public sealed class BrokerageInsurerEnablementsEndpoint : CarterModule
{
    public BrokerageInsurerEnablementsEndpoint()
        : base("brokerage-insurer-enablements")
    {
    }

    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/", CreateAsync)
            .Produces<CreateBrokerageInsurerEnablementResponse>(StatusCodes.Status201Created);

        app.MapPut("/{id:guid}", UpdateAsync)
            .Produces<UpdateBrokerageInsurerEnablementResponse>(StatusCodes.Status200OK);

        app.MapPatch("/{id:guid}/status", ChangeStatusAsync)
            .Produces<ChangeBrokerageInsurerEnablementStatusResponse>(StatusCodes.Status200OK);

        app.MapGet("/", ListAsync)
            .Produces<PagedResponse<BrokerageInsurerEnablementListItemResponse>>(StatusCodes.Status200OK);

        app.MapGet("/{id:guid}", GetAsync)
            .Produces<GetBrokerageInsurerEnablementResponse>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> CreateAsync(
        HttpContext httpContext,
        RequestHandler handler,
        ICreateBrokerageInsurerEnablementUseCase useCase,
        IValidator<CreateBrokerageInsurerEnablementRequest> validator,
        CreateBrokerageInsurerEnablementRequest request)
        => await handler.TryHandleAsync(
            httpContext,
            useCase,
            request,
            validator,
            response => Results.Created($"/api/v1/brokerage-insurer-enablements/{response.Id}", response));

    private static async Task<IResult> UpdateAsync(
        HttpContext httpContext,
        RequestHandler handler,
        IUpdateBrokerageInsurerEnablementUseCase useCase,
        IValidator<UpdateBrokerageInsurerEnablementRequest> validator,
        Guid id,
        UpdateBrokerageInsurerEnablementBody body)
        => await handler.TryHandleAsync(
            httpContext,
            useCase,
            new UpdateBrokerageInsurerEnablementRequest(id, body.CalculationEngine, body.ConnectionParameters),
            validator);

    private static async Task<IResult> ChangeStatusAsync(
        HttpContext httpContext,
        RequestHandler handler,
        IChangeBrokerageInsurerEnablementStatusUseCase useCase,
        IValidator<ChangeBrokerageInsurerEnablementStatusRequest> validator,
        Guid id,
        ChangeBrokerageInsurerEnablementStatusBody body)
        => await handler.TryHandleAsync(
            httpContext,
            useCase,
            new ChangeBrokerageInsurerEnablementStatusRequest(id, body.Status),
            validator);

    private static async Task<IResult> ListAsync(
        HttpContext httpContext,
        RequestHandler handler,
        IListBrokerageInsurerEnablementsUseCase useCase,
        Guid? brokerageId,
        int? page,
        int? pageSize)
        => await handler.TryHandleAsync(
            httpContext,
            useCase,
            new ListBrokerageInsurerEnablementsRequest
            {
                BrokerageId = brokerageId,
                Page = page ?? 1,
                PageSize = pageSize ?? 20,
            });

    private static async Task<IResult> GetAsync(
        HttpContext httpContext,
        RequestHandler handler,
        IGetBrokerageInsurerEnablementUseCase useCase,
        Guid id)
        => await handler.TryHandleAsync(httpContext, useCase, new GetBrokerageInsurerEnablementRequest(id));
}

/// <summary>Corpo do PUT — o id vem da rota.</summary>
public sealed record UpdateBrokerageInsurerEnablementBody(
    string CalculationEngine, string? ConnectionParameters);

/// <summary>Corpo do PATCH de situação — nome estável Active/Inactive.</summary>
public sealed record ChangeBrokerageInsurerEnablementStatusBody(string Status);

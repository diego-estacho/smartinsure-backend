using Carter;
using FluentValidation;
using SmartInsure.Api.Handlers.Base;
using SmartInsure.Application.UseCase.ModelsBase;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.ChangeBrokerageStatus.Interfaces;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.ChangeBrokerageStatus.Requests;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.ChangeBrokerageStatus.Responses;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.CreateBrokerage.Interfaces;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.CreateBrokerage.Requests;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.CreateBrokerage.Responses;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.GetBrokerage.Interfaces;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.GetBrokerage.Requests;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.GetBrokerage.Responses;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.ListBrokerages.Interfaces;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.ListBrokerages.Requests;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.ListBrokerages.Responses;

namespace SmartInsure.Api.Endpoints;

/// <summary>
/// Jornada Corretoras (RN-018..RN-021): qualquer Usuário autenticado lista, cria,
/// detalha e altera situação de Corretoras nesta fase.
/// </summary>
public sealed class BrokeragesEndpoint : CarterModule
{
    public BrokeragesEndpoint()
        : base("brokerages")
    {
    }

    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/", ListAsync)
            .Produces<PagedResponse<BrokerageListItemResponse>>(StatusCodes.Status200OK);

        app.MapGet("/{id:guid}", GetAsync)
            .Produces<GetBrokerageResponse>(StatusCodes.Status200OK);

        app.MapPost("/", CreateAsync)
            .Produces<CreateBrokerageResponse>(StatusCodes.Status201Created);

        app.MapPatch("/{id:guid}/status", ChangeStatusAsync)
            .Produces<ChangeBrokerageStatusResponse>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> ListAsync(
        HttpContext httpContext,
        RequestHandler handler,
        IListBrokeragesUseCase useCase,
        int? page,
        int? pageSize,
        string? status)
        => await handler.TryHandleAsync(
            httpContext,
            useCase,
            new ListBrokeragesRequest
            {
                Page = page ?? 1,
                PageSize = pageSize ?? 20,
                Status = status,
            });

    private static async Task<IResult> GetAsync(
        HttpContext httpContext,
        RequestHandler handler,
        IGetBrokerageUseCase useCase,
        Guid id)
        => await handler.TryHandleAsync(httpContext, useCase, new GetBrokerageRequest(id));

    private static async Task<IResult> CreateAsync(
        HttpContext httpContext,
        RequestHandler handler,
        ICreateBrokerageUseCase useCase,
        IValidator<CreateBrokerageRequest> validator,
        CreateBrokerageRequest request)
        => await handler.TryHandleAsync(
            httpContext,
            useCase,
            request,
            validator,
            response => Results.Created($"/api/v1/brokerages/{response.Id}", response));

    private static async Task<IResult> ChangeStatusAsync(
        HttpContext httpContext,
        RequestHandler handler,
        IChangeBrokerageStatusUseCase useCase,
        IValidator<ChangeBrokerageStatusRequest> validator,
        Guid id,
        ChangeBrokerageStatusBody body)
        => await handler.TryHandleAsync(
            httpContext,
            useCase,
            new ChangeBrokerageStatusRequest(id, body.Status),
            validator);
}

public sealed record ChangeBrokerageStatusBody(string Status);

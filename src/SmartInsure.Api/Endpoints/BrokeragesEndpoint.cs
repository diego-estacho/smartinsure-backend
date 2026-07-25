using Carter;
using FluentValidation;
using SmartInsure.Api.Handlers.Base;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.ChangeBrokerageStatus.Interfaces;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.ChangeBrokerageStatus.Requests;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.ChangeBrokerageStatus.Responses;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.CreateBrokerage.Interfaces;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.CreateBrokerage.Requests;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.CreateBrokerage.Responses;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.GetBrokerage.Interfaces;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.GetBrokerage.Requests;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.GetBrokerage.Responses;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.GetBrokerageHistory.Interfaces;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.GetBrokerageHistory.Requests;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.GetBrokerageHistory.Responses;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.ListBrokerages.Interfaces;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.ListBrokerages.Requests;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.ListBrokerages.Responses;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.PreviewBrokerageByCnpj.Interfaces;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.PreviewBrokerageByCnpj.Requests;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.PreviewBrokerageByCnpj.Responses;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.UpdateBrokerage.Interfaces;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.UpdateBrokerage.Requests;

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
            .Produces<ListBrokeragesResponse>(StatusCodes.Status200OK);

        // RN-032: consulta de CNPJ somente leitura (rota literal antes de /{id:guid}).
        app.MapGet("/preview", PreviewAsync)
            .Produces<BrokeragePreviewResponse>(StatusCodes.Status200OK);

        app.MapGet("/{id:guid}", GetAsync)
            .Produces<GetBrokerageResponse>(StatusCodes.Status200OK);

        app.MapGet("/{id:guid}/history", GetHistoryAsync)
            .Produces<GetBrokerageHistoryResponse>(StatusCodes.Status200OK);

        app.MapPost("/", CreateAsync)
            .Produces<CreateBrokerageResponse>(StatusCodes.Status201Created);

        app.MapPatch("/{id:guid}", UpdateAsync)
            .Produces<GetBrokerageResponse>(StatusCodes.Status200OK);

        app.MapPatch("/{id:guid}/status", ChangeStatusAsync)
            .Produces<ChangeBrokerageStatusResponse>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> ListAsync(
        HttpContext httpContext,
        RequestHandler handler,
        IListBrokeragesUseCase useCase,
        int? page,
        int? pageSize,
        string? q,
        string? situation,
        Guid? insurerId,
        string? calculationEngine,
        string? sector,
        DateTime? registeredFrom,
        DateTime? registeredTo)
        => await handler.TryHandleAsync(
            httpContext,
            useCase,
            new ListBrokeragesRequest
            {
                Page = page ?? 1,
                PageSize = pageSize ?? 20,
                Search = q,
                Situation = situation,
                InsurerId = insurerId,
                CalculationEngine = calculationEngine,
                Sector = sector,
                RegisteredFrom = registeredFrom,
                RegisteredTo = registeredTo,
            });

    private static async Task<IResult> PreviewAsync(
        HttpContext httpContext,
        RequestHandler handler,
        IPreviewBrokerageByCnpjUseCase useCase,
        IValidator<PreviewBrokerageByCnpjRequest> validator,
        string? cnpj)
        => await handler.TryHandleAsync(
            httpContext,
            useCase,
            new PreviewBrokerageByCnpjRequest(cnpj ?? string.Empty),
            validator);

    private static async Task<IResult> GetAsync(
        HttpContext httpContext,
        RequestHandler handler,
        IGetBrokerageUseCase useCase,
        Guid id)
        => await handler.TryHandleAsync(httpContext, useCase, new GetBrokerageRequest(id));

    private static async Task<IResult> GetHistoryAsync(
        HttpContext httpContext,
        RequestHandler handler,
        IGetBrokerageHistoryUseCase useCase,
        Guid id)
        => await handler.TryHandleAsync(httpContext, useCase, new GetBrokerageHistoryRequest(id));

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

    private static async Task<IResult> UpdateAsync(
        HttpContext httpContext,
        RequestHandler handler,
        IUpdateBrokerageUseCase useCase,
        IValidator<UpdateBrokerageRequest> validator,
        Guid id,
        UpdateBrokerageBody body)
        => await handler.TryHandleAsync(
            httpContext,
            useCase,
            new UpdateBrokerageRequest(
                id,
                body.SocialName,
                body.ContactEmail,
                body.ContactPhone,
                body.ResponsibleName),
            validator);

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

public sealed record UpdateBrokerageBody(
    string? SocialName,
    string? ContactEmail,
    string? ContactPhone,
    string? ResponsibleName);

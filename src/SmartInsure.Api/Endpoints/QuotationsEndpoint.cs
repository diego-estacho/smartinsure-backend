using Carter;
using SmartInsure.Api.Handlers.Base;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.GetQuotationsStatus.Interfaces;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.GetQuotationsStatus.Requests;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.GetQuotationsStatus.Responses;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.RunQuotations.Interfaces;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.RunQuotations.Requests;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.RunQuotations.Responses;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.SelectQuotation.Interfaces;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.SelectQuotation.Requests;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.SelectQuotation.Responses;

namespace SmartInsure.Api.Endpoints;

/// <summary>
/// Etapa de cotações (RN-056..061): solicita as Cotações do Grupo às Seguradoras (assíncrono, 202),
/// acompanha por polling e escolhe uma para seguir. Nesta fase, qualquer usuário autenticado (OPEN-03).
/// </summary>
public sealed class QuotationsEndpoint : CarterModule
{
    public QuotationsEndpoint()
        : base("quotation-groups")
    {
    }

    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/{groupId:guid}/quotations", RunAsync)
            .WithName("RunQuotations")
            .WithSummary("Solicita as Cotações do Grupo às Seguradoras (assíncrono)")
            .Produces<RunQuotationsResponse>(StatusCodes.Status202Accepted)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status422UnprocessableEntity);

        app.MapGet("/{groupId:guid}/quotations", StatusAsync)
            .WithName("GetQuotationsStatus")
            .WithSummary("Estado do fan-out do Grupo (progresso + Cotações) — polling")
            .Produces<QuotationsStatusResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        app.MapPost("/{groupId:guid}/quotations/{quotationId:guid}/select", SelectAsync)
            .WithName("SelectQuotation")
            .WithSummary("Escolhe uma Cotação seguível do Grupo para seguir")
            .Produces<SelectQuotationResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status422UnprocessableEntity);
    }

    private static async Task<IResult> RunAsync(
        HttpContext httpContext,
        RequestHandler handler,
        IRunQuotationsUseCase useCase,
        Guid groupId,
        RunQuotationsBody body)
        => await handler.TryHandleAsync(
            httpContext,
            useCase,
            new RunQuotationsRequest(groupId, body.BrokerageId),
            resultFactory: response => Results.Accepted(
                $"/api/v1/quotation-groups/{response.QuotationGroupId}/quotations", response));

    private static async Task<IResult> StatusAsync(
        HttpContext httpContext,
        RequestHandler handler,
        IGetQuotationsStatusUseCase useCase,
        Guid groupId)
        => await handler.TryHandleAsync(httpContext, useCase, new GetQuotationsStatusRequest(groupId));

    private static async Task<IResult> SelectAsync(
        HttpContext httpContext,
        RequestHandler handler,
        ISelectQuotationUseCase useCase,
        Guid groupId,
        Guid quotationId)
        => await handler.TryHandleAsync(httpContext, useCase, new SelectQuotationRequest(groupId, quotationId));
}

/// <summary>Corpo do disparo de cotação: a Corretora que solicita (resolve Habilitações e CNPJ do broker).</summary>
public sealed record RunQuotationsBody(Guid BrokerageId);

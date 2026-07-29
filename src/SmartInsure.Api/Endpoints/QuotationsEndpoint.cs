using Carter;
using SmartInsure.Api.Handlers.Base;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.GetQuotationMinuta.Interfaces;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.GetQuotationMinuta.Requests;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.GetQuotationMinuta.Responses;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.ListQuotations.Interfaces;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.ListQuotations.Requests;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.ListQuotations.Responses;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.RunQuotations.Interfaces;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.RunQuotations.Requests;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.RunQuotations.Responses;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.SelectQuotation.Interfaces;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.SelectQuotation.Requests;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.SelectQuotation.Responses;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.SubmitQuotationTerms.Interfaces;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.SubmitQuotationTerms.Requests;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.SubmitQuotationTerms.Responses;

namespace SmartInsure.Api.Endpoints;

/// <summary>
/// Etapa de cotações (RN-056..063): solicita as Cotações de um Grupo (fan-out, 202), acompanha o leque
/// (polling), seleciona uma seguível e lê a minuta da selecionada. Nesta fase qualquer usuário
/// autenticado (OPEN-03). Emissão e o envio da minuta (RN-063) seguem em demanda própria.
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
            .Produces<RunQuotationsResponse>(StatusCodes.Status202Accepted);

        app.MapGet("/{groupId:guid}/quotations", ListAsync)
            .Produces<ListQuotationsResponse>(StatusCodes.Status200OK);

        app.MapPost("/{groupId:guid}/quotations/{quotationId:guid}/select", SelectAsync)
            .Produces<SelectQuotationResponse>(StatusCodes.Status200OK);

        app.MapGet("/{groupId:guid}/quotations/{quotationId:guid}/minuta", GetMinutaAsync)
            .Produces<QuotationMinutaResponse>(StatusCodes.Status200OK);

        app.MapPost("/{groupId:guid}/quotations/{quotationId:guid}/minuta/submit", SubmitMinutaAsync)
            .Produces<SubmitQuotationTermsResponse>(StatusCodes.Status200OK);
    }

    // RN-056/057: dispara o fan-out e retorna 202 apontando o GET de acompanhamento (ADR-051).
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
                $"/api/v1/quotation-groups/{groupId}/quotations", response));

    // RN-057: leitura barata do estado persistido (leque) para o polling.
    private static async Task<IResult> ListAsync(
        HttpContext httpContext,
        RequestHandler handler,
        IListQuotationsUseCase useCase,
        Guid groupId)
        => await handler.TryHandleAsync(httpContext, useCase, new ListQuotationsRequest(groupId));

    // RN-059: marca a Cotação escolhida do Grupo.
    private static async Task<IResult> SelectAsync(
        HttpContext httpContext,
        RequestHandler handler,
        ISelectQuotationUseCase useCase,
        Guid groupId,
        Guid quotationId)
        => await handler.TryHandleAsync(httpContext, useCase, new SelectQuotationRequest(groupId, quotationId));

    // RN-062: Tags + Cláusulas particulares da minuta da Cotação selecionada.
    private static async Task<IResult> GetMinutaAsync(
        HttpContext httpContext,
        RequestHandler handler,
        IGetQuotationMinutaUseCase useCase,
        Guid groupId,
        Guid quotationId)
        => await handler.TryHandleAsync(httpContext, useCase, new GetQuotationMinutaRequest(quotationId));

    // RN-063: envia os termos preenchidos (Tags + Cláusulas) e devolve a minuta ("Baixar minuta").
    private static async Task<IResult> SubmitMinutaAsync(
        HttpContext httpContext,
        RequestHandler handler,
        ISubmitQuotationTermsUseCase useCase,
        Guid groupId,
        Guid quotationId,
        SubmitQuotationMinutaBody body)
        => await handler.TryHandleAsync(
            httpContext,
            useCase,
            new SubmitQuotationTermsRequest(quotationId, body.BrokerageId, body.Terms, body.ParticularClauses));
}

/// <summary>Corpo do POST de solicitação — a Corretora dona das Habilitações (fonte OPEN-03).</summary>
public sealed record RunQuotationsBody(Guid BrokerageId);

/// <summary>
/// Corpo do POST de "Baixar minuta" (RN-063): a Corretora dona da Habilitação + os termos preenchidos
/// (Tags do objeto) e as Cláusulas particulares marcadas com suas Tags.
/// </summary>
public sealed record SubmitQuotationMinutaBody(
    Guid BrokerageId,
    IReadOnlyList<QuotationTermInput> Terms,
    IReadOnlyList<QuotationClauseInput> ParticularClauses);

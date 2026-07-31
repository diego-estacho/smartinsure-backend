using Carter;
using SmartInsure.Api.Handlers.Base;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.ListQuotationBook.Interfaces;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.ListQuotationBook.Requests;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.ListQuotationBook.Responses;
using SmartInsure.Core.Abstractions.Services;

namespace SmartInsure.Api.Endpoints;

/// <summary>
/// Listagem de Cotações (RN-077): o "livro" da Corretora do Escopo ativo — uma linha por Cotação
/// achatando os Grupos, read-only. A Corretora vem do acesso (RN-064), nunca da query (SECURITY.md).
/// Distinto do <see cref="QuotationsEndpoint"/> (que opera Cotações dentro de UM Grupo).
/// </summary>
public sealed class QuotationBookEndpoint : CarterModule
{
    public QuotationBookEndpoint()
        : base("quotations")
    {
    }

    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/", ListAsync)
            .RequireAuthorization()
            .Produces<QuotationBookResponse>(StatusCodes.Status200OK);
    }

    // RN-077: página do livro com busca + filtro de situação; a Corretora ativa vem do acesso (RN-064).
    private static async Task<IResult> ListAsync(
        HttpContext httpContext,
        RequestHandler handler,
        IListQuotationBookUseCase useCase,
        ICurrentUserAccessor currentUser,
        int? page,
        int? pageSize,
        string? search,
        string? situation)
        => await handler.TryHandleAsync(
            httpContext,
            useCase,
            new ListQuotationBookRequest
            {
                ActiveBrokerageId = currentUser.ActiveBrokerageId,
                Page = page ?? 1,
                PageSize = pageSize ?? 20,
                Search = search,
                Situation = situation,
            });
}

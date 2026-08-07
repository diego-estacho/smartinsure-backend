using Carter;
using SmartInsure.Api.Handlers.Base;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.GetQuotationDetail.Interfaces;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.GetQuotationDetail.Requests;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.GetQuotationDetail.Responses;
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

        app.MapGet("/{id:guid}", GetAsync)
            .RequireAuthorization()
            .Produces<QuotationDetailResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
    }

    // RN-077: página do livro com busca + situação + filtros avançados; a Corretora ativa vem do acesso (RN-064).
    private static async Task<IResult> ListAsync(
        HttpContext httpContext,
        RequestHandler handler,
        IListQuotationBookUseCase useCase,
        ICurrentUserAccessor currentUser,
        int? page,
        int? pageSize,
        string? search,
        string? situation,
        Guid? insurerId,
        Guid? modalityId,
        decimal? premiumMin,
        decimal? premiumMax,
        decimal? insuredAmountMin,
        decimal? insuredAmountMax,
        DateOnly? createdFrom,
        DateOnly? createdTo,
        DateOnly? coverageStartFrom,
        DateOnly? coverageStartTo)
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
                InsurerId = insurerId,
                ModalityId = modalityId,
                PremiumMin = premiumMin,
                PremiumMax = premiumMax,
                InsuredAmountMin = insuredAmountMin,
                InsuredAmountMax = insuredAmountMax,
                CreatedFrom = createdFrom,
                CreatedTo = createdTo,
                CoverageStartFrom = coverageStartFrom,
                CoverageStartTo = coverageStartTo,
            });

    // RN-081: detalhe read-only por identidade (guid), nunca por número; a Corretora ativa vem do acesso
    // (RN-064). Cotação de outra Corretora (ou id inexistente) → 404 idêntico (não revela existência).
    private static async Task<IResult> GetAsync(
        HttpContext httpContext,
        RequestHandler handler,
        IGetQuotationDetailUseCase useCase,
        ICurrentUserAccessor currentUser,
        Guid id)
        => await handler.TryHandleAsync(
            httpContext,
            useCase,
            new GetQuotationDetailRequest(id, currentUser.ActiveBrokerageId));
}

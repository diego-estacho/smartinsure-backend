using Carter;
using FluentValidation;
using SmartInsure.Api.Handlers.Base;
using SmartInsure.Application.UseCase.ModelsBase;
using SmartInsure.Application.UseCase.UseCases.CreditInquiryUseCases.ExecuteCreditInquiry.Interfaces;
using SmartInsure.Application.UseCase.UseCases.CreditInquiryUseCases.ExecuteCreditInquiry.Requests;
using SmartInsure.Application.UseCase.UseCases.CreditInquiryUseCases.ExecuteCreditInquiry.Responses;
using SmartInsure.Application.UseCase.UseCases.CreditInquiryUseCases.GetCreditInquiry.Interfaces;
using SmartInsure.Application.UseCase.UseCases.CreditInquiryUseCases.GetCreditInquiry.Requests;
using SmartInsure.Application.UseCase.UseCases.CreditInquiryUseCases.GetCreditInquiry.Responses;
using SmartInsure.Application.UseCase.UseCases.CreditInquiryUseCases.ListCreditInquiries.Interfaces;
using SmartInsure.Application.UseCase.UseCases.CreditInquiryUseCases.ListCreditInquiries.Requests;
using SmartInsure.Application.UseCase.UseCases.CreditInquiryUseCases.ListCreditInquiries.Responses;

namespace SmartInsure.Api.Endpoints;

/// <summary>
/// Jornada Consulta de Crédito (RN-029..031): consulta de limites de crédito do tomador junto
/// às Seguradoras habilitadas da Corretora, com histórico e rastreabilidade.
/// Nesta fase, qualquer usuário autenticado pode executar consultas (OPEN-03).
/// </summary>
public sealed class CreditInquiriesEndpoint : CarterModule
{
    public CreditInquiriesEndpoint()
        : base("credit-inquiries")
    {
    }

    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/", ExecuteAsync)
            .WithName("ExecuteCreditInquiry")
            .WithSummary("Executa consulta de limites de crédito do tomador")
            .Produces<ExecuteCreditInquiryResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status422UnprocessableEntity);

        app.MapGet("/{id:guid}", GetAsync)
            .WithName("GetCreditInquiry")
            .WithSummary("Recupera histórico de uma consulta de crédito")
            .Produces<GetCreditInquiryResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        app.MapGet("/", ListAsync)
            .WithName("ListCreditInquiries")
            .WithSummary("Lista histórico de consultas de crédito (paginado)")
            .Produces<PagedResponse<CreditInquiryListItemResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> ExecuteAsync(
        HttpContext httpContext,
        RequestHandler handler,
        IExecuteCreditInquiryUseCase useCase,
        IValidator<ExecuteCreditInquiryRequest> validator,
        ExecuteCreditInquiryRequest request)
        => await handler.TryHandleAsync(httpContext, useCase, request, validator);

    private static async Task<IResult> GetAsync(
        HttpContext httpContext,
        RequestHandler handler,
        IGetCreditInquiryUseCase useCase,
        Guid id)
        => await handler.TryHandleAsync(httpContext, useCase, new GetCreditInquiryRequest(id));

    private static async Task<IResult> ListAsync(
        HttpContext httpContext,
        RequestHandler handler,
        IListCreditInquiriesUseCase useCase,
        Guid? brokerageId,
        string? policyHolderCnpj,
        int? page,
        int? pageSize)
        => await handler.TryHandleAsync(
            httpContext,
            useCase,
            new ListCreditInquiriesRequest
            {
                BrokerageId = brokerageId,
                PolicyHolderCnpj = policyHolderCnpj,
                Page = page ?? 1,
                PageSize = pageSize ?? 20,
            });
}

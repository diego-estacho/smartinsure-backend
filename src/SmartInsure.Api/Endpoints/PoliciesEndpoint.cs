using Carter;
using SmartInsure.Api.Handlers.Base;
using SmartInsure.Application.UseCase.UseCases.IssuanceUseCases.GetInsurerTerm.Interfaces;
using SmartInsure.Application.UseCase.UseCases.IssuanceUseCases.GetInsurerTerm.Requests;
using SmartInsure.Application.UseCase.UseCases.IssuanceUseCases.GetInsurerTerm.Responses;
using SmartInsure.Application.UseCase.UseCases.IssuanceUseCases.RequestPolicyIssuance.Interfaces;
using SmartInsure.Application.UseCase.UseCases.IssuanceUseCases.RequestPolicyIssuance.Requests;
using SmartInsure.Application.UseCase.UseCases.IssuanceUseCases.RequestPolicyIssuance.Responses;
using SmartInsure.Application.UseCase.UseCases.IssuanceUseCases.UpdateQuotationTax.Interfaces;
using SmartInsure.Application.UseCase.UseCases.IssuanceUseCases.UpdateQuotationTax.Requests;
using SmartInsure.Application.UseCase.UseCases.IssuanceUseCases.UpdateQuotationTax.Responses;

namespace SmartInsure.Api.Endpoints;

/// <summary>
/// Etapa de emissão (RN-500..RN-514): ajusta a taxa da Cotação escolhida e solicita a emissão da
/// Apólice. A resposta confirma que a emissão foi **solicitada** — número da apólice, arquivo e boletos
/// vêm da confirmação junto à Seguradora, que é demanda própria (OPEN-07).
///
/// A Permissão de emitir está declarada no catálogo (RN-513), mas o enforcement por Permissão ainda não
/// existe em nenhuma rota da plataforma (OPEN-03): nesta fase vale qualquer usuário autenticado, como no
/// resto da API. Ligar o enforcement é trabalho da jornada de Perfis, não desta.
/// </summary>
public sealed class PoliciesEndpoint : CarterModule
{
    public PoliciesEndpoint()
        : base("quotation-groups")
    {
    }

    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/{groupId:guid}/policy", RequestIssuanceAsync)
            .Produces<RequestPolicyIssuanceResponse>(StatusCodes.Status201Created);

        app.MapPost("/{groupId:guid}/quotations/selected/tax", UpdateTaxAsync)
            .Produces<UpdateQuotationTaxResponse>(StatusCodes.Status200OK);

        app.MapGet("/{groupId:guid}/insurer-term", GetInsurerTermAsync)
            .Produces<GetInsurerTermResponse>(StatusCodes.Status200OK);
    }

    // RN-506: texto vigente do Termo da Seguradora da Cotação escolhida — o mesmo que o aceite registra.
    private static async Task<IResult> GetInsurerTermAsync(
        HttpContext httpContext,
        RequestHandler handler,
        IGetInsurerTermUseCase useCase,
        Guid groupId)
        => await handler.TryHandleAsync(httpContext, useCase, new GetInsurerTermRequest(groupId));

    // RN-500/RN-514: portão de verificações + sequência do emitir; 201 com a Apólice registrada.
    private static async Task<IResult> RequestIssuanceAsync(
        HttpContext httpContext,
        RequestHandler handler,
        IRequestPolicyIssuanceUseCase useCase,
        Guid groupId,
        RequestPolicyIssuanceBody body)
        => await handler.TryHandleAsync(
            httpContext,
            useCase,
            new RequestPolicyIssuanceRequest
            {
                QuotationGroupId = groupId,
                InstallmentNumber = body.InstallmentNumber,
                GracePeriodInDays = body.GracePeriodInDays,
                TermAccepted = body.TermAccepted,
                // RN-506: o agente de acesso vem da borda e integra a prova do aceite.
                UserAgent = httpContext.Request.Headers.UserAgent.ToString(),
            },
            resultFactory: response => Results.Created(
                $"/api/v1/quotation-groups/{groupId}/policy", response));

    // RN-504: taxa nova submetida à Seguradora; a resposta traz os valores que ela devolveu.
    private static async Task<IResult> UpdateTaxAsync(
        HttpContext httpContext,
        RequestHandler handler,
        IUpdateQuotationTaxUseCase useCase,
        Guid groupId,
        UpdateQuotationTaxBody body)
        => await handler.TryHandleAsync(
            httpContext,
            useCase,
            new UpdateQuotationTaxRequest { QuotationGroupId = groupId, Tax = body.Tax });
}

/// <summary>
/// Corpo do pedido de emissão (RN-505/RN-506): parcelamento e vencimento escolhidos entre as opções da
/// Cotação, e o aceite explícito do Termo.
/// </summary>
public sealed record RequestPolicyIssuanceBody(int InstallmentNumber, int GracePeriodInDays, bool TermAccepted);

/// <summary>Corpo do ajuste de taxa (RN-504).</summary>
public sealed record UpdateQuotationTaxBody(decimal Tax);

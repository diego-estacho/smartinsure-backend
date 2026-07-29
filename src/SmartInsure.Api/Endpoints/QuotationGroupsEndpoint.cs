using Carter;
using FluentValidation;
using SmartInsure.Api.Handlers.Base;
using SmartInsure.Application.UseCase.UseCases.QuotationGroupUseCases.CreateQuotationGroup.Interfaces;
using SmartInsure.Application.UseCase.UseCases.QuotationGroupUseCases.CreateQuotationGroup.Requests;
using SmartInsure.Application.UseCase.UseCases.QuotationGroupUseCases.CreateQuotationGroup.Responses;
using SmartInsure.Application.UseCase.UseCases.QuotationGroupUseCases.UpdateQuotationGroup.Interfaces;
using SmartInsure.Application.UseCase.UseCases.QuotationGroupUseCases.UpdateQuotationGroup.Requests;
using SmartInsure.Application.UseCase.UseCases.QuotationGroupUseCases.UpdateQuotationGroup.Responses;

namespace SmartInsure.Api.Endpoints;

/// <summary>
/// Jornada Grupo de Cotação (RN-050/RN-051): salva o pedido do corretor (a "nova oferta") em Rascunho.
/// POST cria ao concluir a etapa de risco; PUT atualiza no lugar enquanto Rascunho. Nesta fase qualquer
/// usuário autenticado (OPEN-03/OPEN-07). Cotar as Seguradoras e emitir seguem fora de escopo.
/// </summary>
public sealed class QuotationGroupsEndpoint : CarterModule
{
    public QuotationGroupsEndpoint()
        : base("quotation-groups")
    {
    }

    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/", CreateAsync)
            .Produces<CreateQuotationGroupResponse>(StatusCodes.Status201Created);

        app.MapPut("/{id:guid}", UpdateAsync)
            .Produces<UpdateQuotationGroupResponse>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> CreateAsync(
        HttpContext httpContext,
        RequestHandler handler,
        ICreateQuotationGroupUseCase useCase,
        IValidator<CreateQuotationGroupRequest> validator,
        CreateQuotationGroupRequest request)
        => await handler.TryHandleAsync(
            httpContext,
            useCase,
            request,
            validator,
            response => Results.Created($"/api/v1/quotation-groups/{response.Id}", response));

    private static async Task<IResult> UpdateAsync(
        HttpContext httpContext,
        RequestHandler handler,
        IUpdateQuotationGroupUseCase useCase,
        IValidator<UpdateQuotationGroupRequest> validator,
        Guid id,
        UpdateQuotationGroupBody body)
        => await handler.TryHandleAsync(
            httpContext,
            useCase,
            new UpdateQuotationGroupRequest(
                id,
                body.PolicyHolderId,
                body.InsuredId,
                body.ModalityId,
                body.InsuredAmount,
                body.CoverageStartDate,
                body.CoverageEndDate,
                body.ScopeMode,
                body.InsurerIds,
                body.IncludesPenaltyCoverage,
                body.IncludesLaborCoverage),
            validator);
}

/// <summary>Corpo do PUT do Grupo de Cotação — o id vem da rota.</summary>
public sealed record UpdateQuotationGroupBody(
    Guid PolicyHolderId,
    Guid InsuredId,
    Guid ModalityId,
    decimal InsuredAmount,
    DateOnly CoverageStartDate,
    DateOnly CoverageEndDate,
    string ScopeMode,
    IReadOnlyList<Guid> InsurerIds,
    bool IncludesPenaltyCoverage,
    bool IncludesLaborCoverage);

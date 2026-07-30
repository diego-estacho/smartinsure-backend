using Carter;
using SmartInsure.Api.Handlers.Base;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.GetCurrentUserContext.Interfaces;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.GetCurrentUserContext.Requests;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.GetCurrentUserContext.Responses;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.SwitchActiveScope.Interfaces;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.SwitchActiveScope.Requests;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.SwitchActiveScope.Responses;
using SmartInsure.Core.Abstractions.Services;

namespace SmartInsure.Api.Endpoints;

/// <summary>
/// Contexto do Usuário autenticado (RN-064): onde ele pode operar (Vínculos) e onde está
/// operando (Escopo ativo), além da troca de Escopo. Qualquer Usuário autenticado usa estas
/// rotas — elas só falam do próprio acesso, nunca do de outro.
/// </summary>
public sealed class MeEndpoint : CarterModule
{
    public MeEndpoint()
        : base("me")
    {
    }

    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/", GetAsync)
            .Produces<GetCurrentUserContextResponse>(StatusCodes.Status200OK);

        // RN-064/ADR-065: a troca reemite o acesso — o cliente troca o token que guarda.
        app.MapPost("/active-scope", SwitchScopeAsync)
            .Produces<SwitchActiveScopeResponse>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> GetAsync(
        HttpContext httpContext,
        RequestHandler handler,
        IGetCurrentUserContextUseCase useCase,
        ICurrentUserAccessor currentUser)
        => await handler.TryHandleAsync(
            httpContext,
            useCase,
            new GetCurrentUserContextRequest(
                currentUser.UserIdentifier ?? string.Empty,
                currentUser.ActiveBrokerageId,
                currentUser.ActivePolicyHolderId));

    private static async Task<IResult> SwitchScopeAsync(
        HttpContext httpContext,
        RequestHandler handler,
        ISwitchActiveScopeUseCase useCase,
        ICurrentUserAccessor currentUser,
        SwitchActiveScopeBody body)
        => await handler.TryHandleAsync(
            httpContext,
            useCase,
            new SwitchActiveScopeRequest(
                currentUser.UserIdentifier ?? string.Empty,
                body.BrokerageId,
                body.PolicyHolderId));
}

/// <summary>Corpo da troca de Escopo (RN-064): nulo em um dos lados = sair daquele Escopo.</summary>
public sealed record SwitchActiveScopeBody(Guid? BrokerageId, Guid? PolicyHolderId);

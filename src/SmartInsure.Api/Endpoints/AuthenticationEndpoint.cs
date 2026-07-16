using Carter;
using FluentValidation;
using SmartInsure.Api.Handlers.Base;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.AuthenticateUser.Interfaces;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.AuthenticateUser.Requests;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.AuthenticateUser.Responses;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.LogoutUser.Interfaces;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.LogoutUser.Requests;
using SmartInsure.Core.Abstractions.Services;

namespace SmartInsure.Api.Endpoints;

/// <summary>Jornada Usuários: RN-005 (autenticação) e RN-006 (encerramento de sessão).</summary>
public sealed class AuthenticationEndpoint : CarterModule
{
    public AuthenticationEndpoint()
        : base("auth")
    {
    }

    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        // RN-005: única rota anônima do grupo — o login é a porta de entrada (ADR-010).
        app.MapPost("/login", LoginAsync)
            .AllowAnonymous()
            .Produces<AuthenticateUserResponse>(StatusCodes.Status200OK);

        app.MapPost("/logout", LogoutAsync)
            .Produces(StatusCodes.Status204NoContent);
    }

    /// <summary>
    /// RN-006: o acesso a encerrar vem do token do próprio chamador — o cliente não
    /// decide qual sessão encerra (SECURITY.md).
    /// </summary>
    private static async Task<IResult> LogoutAsync(
        HttpContext httpContext,
        RequestHandler handler,
        ILogoutUserUseCase useCase,
        ICurrentUserAccessor currentUser)
        => await handler.TryHandleAsync(
            httpContext,
            useCase,
            new LogoutUserRequest(currentUser.SessionTokenId, currentUser.SessionExpiresAtUtc),
            resultFactory: _ => Results.NoContent());

    private static async Task<IResult> LoginAsync(
        HttpContext httpContext,
        RequestHandler handler,
        IAuthenticateUserUseCase useCase,
        IValidator<AuthenticateUserRequest> validator,
        AuthenticateUserRequest request)
        => await handler.TryHandleAsync(httpContext, useCase, request, validator);
}

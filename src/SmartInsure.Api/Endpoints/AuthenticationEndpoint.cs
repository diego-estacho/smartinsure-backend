using Carter;
using FluentValidation;
using SmartInsure.Api.Handlers.Base;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.AuthenticateUser.Interfaces;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.AuthenticateUser.Requests;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.AuthenticateUser.Responses;

namespace SmartInsure.Api.Endpoints;

/// <summary>Jornada Usuários: RN-005 (autenticação com e-mail e senha).</summary>
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
    }

    private static async Task<IResult> LoginAsync(
        HttpContext httpContext,
        RequestHandler handler,
        IAuthenticateUserUseCase useCase,
        IValidator<AuthenticateUserRequest> validator,
        AuthenticateUserRequest request)
        => await handler.TryHandleAsync(httpContext, useCase, request, validator);
}

using Carter;
using FluentValidation;
using SmartInsure.Api.Handlers.Base;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.ActivateUser.Interfaces;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.ActivateUser.Requests;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.ActivateUser.Responses;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.CreateUser.Interfaces;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.CreateUser.Requests;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.CreateUser.Responses;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.SetUserProfile.Interfaces;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.SetUserProfile.Requests;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.SetUserProfile.Responses;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Constants;

namespace SmartInsure.Api.Endpoints;

/// <summary>Jornada Usuários: RN-001 (criação) e RN-002 (ativação no primeiro acesso).</summary>
public sealed class UsersEndpoint : CarterModule
{
    public UsersEndpoint()
        : base("users")
    {
    }

    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/", CreateAsync)
            .Produces<CreateUserResponse>(StatusCodes.Status201Created);

        app.MapPost("/activation", ActivateAsync)
            .Produces<ActivateUserResponse>(StatusCodes.Status200OK);

        app.MapPut("/{id:guid}/profile", SetProfileAsync)
            .RequireAuthorization(Policies.SystemAdministrator)
            .Produces<SetUserProfileResponse>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> CreateAsync(
        HttpContext httpContext,
        RequestHandler handler,
        ICreateUserUseCase useCase,
        IValidator<CreateUserRequest> validator,
        CreateUserRequest request)
        => await handler.TryHandleAsync(
            httpContext,
            useCase,
            request,
            validator,
            response => Results.Created($"/api/v1/users/{response.Id}", response));

    /// <summary>
    /// RN-002: a identidade vem do token do usuário autenticado — o cliente não decide
    /// quem ativa (SECURITY.md: permissão e status são do servidor).
    /// </summary>
    private static async Task<IResult> ActivateAsync(
        HttpContext httpContext,
        RequestHandler handler,
        IActivateUserUseCase useCase,
        ICurrentUserAccessor currentUser)
        => await handler.TryHandleAsync(
            httpContext,
            useCase,
            new ActivateUserRequest(currentUser.UserIdentifier ?? string.Empty));

    /// <summary>RN-012: somente Administrador do Sistema concede/revoga Perfil.</summary>
    private static async Task<IResult> SetProfileAsync(
        HttpContext httpContext,
        RequestHandler handler,
        ISetUserProfileUseCase useCase,
        IValidator<SetUserProfileRequest> validator,
        Guid id,
        SetUserProfileBody body)
        => await handler.TryHandleAsync(
            httpContext,
            useCase,
            new SetUserProfileRequest(id, body.Profile),
            validator);
}

public sealed record SetUserProfileBody(string? Profile);

using Carter;
using FluentValidation;
using SmartInsure.Api.Handlers.Base;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.AcceptInvitation.Interfaces;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.AcceptInvitation.Requests;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.AcceptInvitation.Responses;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.CreateUser.Interfaces;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.CreateUser.Requests;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.CreateUser.Responses;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.ChangeUserActivation.Interfaces;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.ChangeUserActivation.Requests;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.ChangeUserActivation.Responses;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.InviteBrokerageAdministrator.Interfaces;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.InviteBrokerageAdministrator.Requests;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.InviteBrokerageAdministrator.Responses;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.ResendInvitation.Interfaces;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.ResendInvitation.Requests;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.ResendInvitation.Responses;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.SetUserProfile.Interfaces;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.SetUserProfile.Requests;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.SetUserProfile.Responses;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Constants;

namespace SmartInsure.Api.Endpoints;

/// <summary>Jornada Usuários: RN-001 (criação), RN-035 (convite), RN-012 (perfil).</summary>
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

        app.MapPost("/invitations/accept", AcceptInvitationAsync)
            .AllowAnonymous()
            .Produces<AcceptInvitationResponse>(StatusCodes.Status200OK);

        app.MapPost("/{id:guid}/invitations/resend", ResendInvitationAsync)
            .RequireAuthorization()
            .Produces<ResendInvitationResponse>(StatusCodes.Status200OK);

        app.MapPut("/{id:guid}/profile", SetProfileAsync)
            .RequireAuthorization(Policies.SystemAdministrator)
            .Produces<SetUserProfileResponse>(StatusCodes.Status200OK);

        // RN-036: somente o Administrador do Sistema convida Corretor Administrador.
        app.MapPost("/brokerage-administrators", InviteBrokerageAdministratorAsync)
            .RequireAuthorization(Policies.SystemAdministrator)
            .Produces<InviteBrokerageAdministratorResponse>(StatusCodes.Status201Created);

        // RN-046: inativação/reativação de Usuário (nesta fatia, do Administrador do Sistema — [OPEN-12]).
        app.MapPost("/{id:guid}/inactivate", InactivateAsync)
            .RequireAuthorization(Policies.SystemAdministrator)
            .Produces<ChangeUserActivationResponse>(StatusCodes.Status200OK);

        app.MapPost("/{id:guid}/reactivate", ReactivateAsync)
            .RequireAuthorization(Policies.SystemAdministrator)
            .Produces<ChangeUserActivationResponse>(StatusCodes.Status200OK);
    }

    /// <summary>RN-046: inativa um Usuário (Administrador do Sistema).</summary>
    private static async Task<IResult> InactivateAsync(
        HttpContext httpContext,
        RequestHandler handler,
        IChangeUserActivationUseCase useCase,
        Guid id)
        => await handler.TryHandleAsync(
            httpContext, useCase, new ChangeUserActivationRequest(id, Activate: false));

    /// <summary>RN-046: reativa um Usuário (Administrador do Sistema).</summary>
    private static async Task<IResult> ReactivateAsync(
        HttpContext httpContext,
        RequestHandler handler,
        IChangeUserActivationUseCase useCase,
        Guid id)
        => await handler.TryHandleAsync(
            httpContext, useCase, new ChangeUserActivationRequest(id, Activate: true));

    /// <summary>RN-036: o Administrador do Sistema convida um Corretor Administrador para as Corretoras informadas.</summary>
    private static async Task<IResult> InviteBrokerageAdministratorAsync(
        HttpContext httpContext,
        RequestHandler handler,
        IInviteBrokerageAdministratorUseCase useCase,
        IValidator<InviteBrokerageAdministratorRequest> validator,
        InviteBrokerageAdministratorRequest request)
        => await handler.TryHandleAsync(
            httpContext,
            useCase,
            request,
            validator,
            response => Results.Created($"/api/v1/users/{response.Id}", response));

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

    /// <summary>RN-035: primeiro acesso — aceita o convite e define a senha.</summary>
    private static async Task<IResult> AcceptInvitationAsync(
        HttpContext httpContext,
        RequestHandler handler,
        IAcceptInvitationUseCase useCase,
        IValidator<AcceptInvitationRequest> validator,
        AcceptInvitationRequest request)
        => await handler.TryHandleAsync(
            httpContext,
            useCase,
            request,
            validator);

    /// <summary>RN-035: reenvio do convite enquanto Pendente.</summary>
    private static async Task<IResult> ResendInvitationAsync(
        HttpContext httpContext,
        RequestHandler handler,
        IResendInvitationUseCase useCase,
        Guid id)
        => await handler.TryHandleAsync(
            httpContext,
            useCase,
            new ResendInvitationRequest(id));

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

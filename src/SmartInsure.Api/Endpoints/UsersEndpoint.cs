using Carter;
using FluentValidation;
using SmartInsure.Api.Handlers.Base;
using SmartInsure.Application.UseCase.ModelsBase;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.AcceptInvitation.Interfaces;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.AcceptInvitation.Requests;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.AcceptInvitation.Responses;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.CreateUser.Interfaces;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.CreateUser.Requests;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.CreateUser.Responses;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.ChangeUserActivation.Interfaces;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.ChangeUserActivation.Requests;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.ChangeUserActivation.Responses;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.GetUser.Interfaces;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.GetUser.Requests;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.GetUser.Responses;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.InviteBrokerageUser.Interfaces;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.InviteBrokerageUser.Requests;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.InviteBrokerageUser.Responses;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.InvitePolicyHolderUser.Interfaces;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.InvitePolicyHolderUser.Requests;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.InvitePolicyHolderUser.Responses;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.InvitePolicyHolderAdministrator.Interfaces;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.InvitePolicyHolderAdministrator.Requests;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.InvitePolicyHolderAdministrator.Responses;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.ListUsers.Interfaces;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.ListUsers.Requests;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.ListUsers.Responses;
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

/// <summary>Jornada Usuários: RN-001 (criação), RN-065 (convite), RN-012 (perfil).</summary>
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

        // RN-064: a visibilidade é por Escopo — Administrador do Sistema vê todos, administradores
        // de Escopo veem o próprio. A decisão é do use case, não de policy de rota.
        app.MapGet("/", ListAsync)
            .RequireAuthorization()
            .Produces<PagedResponse<UserListItemResponse>>(StatusCodes.Status200OK);

        app.MapGet("/{id:guid}", GetAsync)
            .RequireAuthorization(Policies.SystemAdministrator)
            .Produces<GetUserResponse>(StatusCodes.Status200OK);

        app.MapPost("/invitations/accept", AcceptInvitationAsync)
            .AllowAnonymous()
            .Produces<AcceptInvitationResponse>(StatusCodes.Status200OK);

        app.MapPost("/{id:guid}/invitations/resend", ResendInvitationAsync)
            .RequireAuthorization()
            .Produces<ResendInvitationResponse>(StatusCodes.Status200OK);

        app.MapPut("/{id:guid}/profile", SetProfileAsync)
            .RequireAuthorization(Policies.SystemAdministrator)
            .Produces<SetUserProfileResponse>(StatusCodes.Status200OK);

        // RN-066: somente o Administrador do Sistema convida Corretor Administrador.
        app.MapPost("/brokerage-administrators", InviteBrokerageAdministratorAsync)
            .RequireAuthorization(Policies.SystemAdministrator)
            .Produces<InviteBrokerageAdministratorResponse>(StatusCodes.Status201Created);

        // RN-068/RN-069: o ator é o Corretor Administrador da Corretora ativa — Perfil por Vínculo,
        // então a autorização é conferida no use case (não há policy de rota para isso).
        app.MapPost("/policy-holder-administrators", InvitePolicyHolderAdministratorAsync)
            .RequireAuthorization()
            .Produces<InvitePolicyHolderAdministratorResponse>(StatusCodes.Status201Created);

        app.MapPost("/brokerage-users", InviteBrokerageUserAsync)
            .RequireAuthorization()
            .Produces<InviteBrokerageUserResponse>(StatusCodes.Status201Created);

        // RN-070: o Tomador Administrador cria Usuários do Tomador ativo.
        app.MapPost("/policy-holder-users", InvitePolicyHolderUserAsync)
            .RequireAuthorization()
            .Produces<InvitePolicyHolderUserResponse>(StatusCodes.Status201Created);

        // RN-076: inativação/reativação de Usuário (nesta fatia, do Administrador do Sistema — [OPEN-20]).
        app.MapPost("/{id:guid}/inactivate", InactivateAsync)
            .RequireAuthorization(Policies.SystemAdministrator)
            .Produces<ChangeUserActivationResponse>(StatusCodes.Status200OK);

        app.MapPost("/{id:guid}/reactivate", ReactivateAsync)
            .RequireAuthorization(Policies.SystemAdministrator)
            .Produces<ChangeUserActivationResponse>(StatusCodes.Status200OK);
    }

    /// <summary>
    /// RN-068: o Corretor Administrador cria um Tomador Administrador. A Corretora ativa vem do
    /// acesso — o cliente não escolhe em nome de qual corretora está agindo (SECURITY.md).
    /// </summary>
    private static async Task<IResult> InvitePolicyHolderAdministratorAsync(
        HttpContext httpContext,
        RequestHandler handler,
        IInvitePolicyHolderAdministratorUseCase useCase,
        IValidator<InvitePolicyHolderAdministratorRequest> validator,
        ICurrentUserAccessor currentUser,
        InvitePolicyHolderAdministratorBody body)
        => await handler.TryHandleAsync(
            httpContext,
            useCase,
            new InvitePolicyHolderAdministratorRequest(
                currentUser.UserIdentifier ?? string.Empty,
                currentUser.ActiveBrokerageId,
                body.Name,
                body.Email,
                body.PolicyHolderId),
            validator,
            response => Results.Created($"/api/v1/users/{response.Id}", response));

    /// <summary>RN-069: o Corretor Administrador cria um Usuário na Corretora ativa.</summary>
    private static async Task<IResult> InviteBrokerageUserAsync(
        HttpContext httpContext,
        RequestHandler handler,
        IInviteBrokerageUserUseCase useCase,
        IValidator<InviteBrokerageUserRequest> validator,
        ICurrentUserAccessor currentUser,
        InviteBrokerageUserBody body)
        => await handler.TryHandleAsync(
            httpContext,
            useCase,
            new InviteBrokerageUserRequest(
                currentUser.UserIdentifier ?? string.Empty,
                currentUser.ActiveBrokerageId,
                body.Name,
                body.Email,
                body.ProfileId),
            validator,
            response => Results.Created($"/api/v1/users/{response.Id}", response));

    /// <summary>RN-070: o Tomador Administrador cria um Usuário do Tomador ativo (lido do acesso).</summary>
    private static async Task<IResult> InvitePolicyHolderUserAsync(
        HttpContext httpContext,
        RequestHandler handler,
        IInvitePolicyHolderUserUseCase useCase,
        IValidator<InvitePolicyHolderUserRequest> validator,
        ICurrentUserAccessor currentUser,
        InvitePolicyHolderUserBody body)
        => await handler.TryHandleAsync(
            httpContext,
            useCase,
            new InvitePolicyHolderUserRequest(
                currentUser.UserIdentifier ?? string.Empty,
                currentUser.ActivePolicyHolderId,
                body.Name,
                body.Email,
                body.ProfileId),
            validator,
            response => Results.Created($"/api/v1/users/{response.Id}", response));

    /// <summary>Listagem de Usuários com busca por nome/e-mail e filtro de situação.</summary>
    private static async Task<IResult> ListAsync(
        HttpContext httpContext,
        RequestHandler handler,
        IListUsersUseCase useCase,
        ICurrentUserAccessor currentUser,
        int? page,
        int? pageSize,
        string? search,
        string? status)
        => await handler.TryHandleAsync(
            httpContext,
            useCase,
            new ListUsersRequest
            {
                ExternalIdentity = currentUser.UserIdentifier ?? string.Empty,
                ActiveBrokerageId = currentUser.ActiveBrokerageId,
                ActivePolicyHolderId = currentUser.ActivePolicyHolderId,
                Page = page ?? 1,
                PageSize = pageSize ?? 20,
                Search = search,
                Status = status,
            });

    /// <summary>Detalhe do Usuário: Perfil (RN-012) e Vínculos de Corretora/Tomador (RN-064).</summary>
    private static async Task<IResult> GetAsync(
        HttpContext httpContext,
        RequestHandler handler,
        IGetUserUseCase useCase,
        Guid id)
        => await handler.TryHandleAsync(httpContext, useCase, new GetUserRequest(id));

    /// <summary>RN-076: inativa um Usuário (Administrador do Sistema).</summary>
    private static async Task<IResult> InactivateAsync(
        HttpContext httpContext,
        RequestHandler handler,
        IChangeUserActivationUseCase useCase,
        Guid id)
        => await handler.TryHandleAsync(
            httpContext, useCase, new ChangeUserActivationRequest(id, Activate: false));

    /// <summary>RN-076: reativa um Usuário (Administrador do Sistema).</summary>
    private static async Task<IResult> ReactivateAsync(
        HttpContext httpContext,
        RequestHandler handler,
        IChangeUserActivationUseCase useCase,
        Guid id)
        => await handler.TryHandleAsync(
            httpContext, useCase, new ChangeUserActivationRequest(id, Activate: true));

    /// <summary>RN-066: o Administrador do Sistema convida um Corretor Administrador para as Corretoras informadas.</summary>
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

    /// <summary>RN-065: primeiro acesso — aceita o convite e define a senha.</summary>
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

    /// <summary>RN-065: reenvio do convite enquanto Pendente.</summary>
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

/// <summary>RN-068: corpo do convite de Tomador Administrador (a Corretora ativa vem do acesso).</summary>
public sealed record InvitePolicyHolderAdministratorBody(
    string Name,
    string Email,
    Guid PolicyHolderId);

/// <summary>RN-069: corpo da criação de Usuário na Corretora ativa.</summary>
public sealed record InviteBrokerageUserBody(string Name, string Email, Guid ProfileId);

/// <summary>RN-070: corpo da criação de Usuário no Tomador ativo (o Tomador vem do acesso).</summary>
public sealed record InvitePolicyHolderUserBody(string Name, string Email, Guid ProfileId);

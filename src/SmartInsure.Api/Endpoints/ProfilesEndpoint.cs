using Carter;
using FluentValidation;
using SmartInsure.Api.Handlers.Base;
using SmartInsure.Application.UseCase.ModelsBase;
using SmartInsure.Application.UseCase.UseCases.ProfileUseCases.CreateScopedProfile.Interfaces;
using SmartInsure.Application.UseCase.UseCases.ProfileUseCases.CreateScopedProfile.Requests;
using SmartInsure.Application.UseCase.UseCases.ProfileUseCases.CreateScopedProfile.Responses;
using SmartInsure.Application.UseCase.UseCases.ProfileUseCases.DeleteScopedProfile.Interfaces;
using SmartInsure.Application.UseCase.UseCases.ProfileUseCases.DeleteScopedProfile.Requests;
using SmartInsure.Application.UseCase.UseCases.ProfileUseCases.GetProfile.Interfaces;
using SmartInsure.Application.UseCase.UseCases.ProfileUseCases.GetProfile.Requests;
using SmartInsure.Application.UseCase.UseCases.ProfileUseCases.GetProfile.Responses;
using SmartInsure.Application.UseCase.UseCases.ProfileUseCases.ListAssignableProfiles.Interfaces;
using SmartInsure.Application.UseCase.UseCases.ProfileUseCases.ListAssignableProfiles.Requests;
using SmartInsure.Application.UseCase.UseCases.ProfileUseCases.ListAssignableProfiles.Responses;
using SmartInsure.Application.UseCase.UseCases.ProfileUseCases.ListProfiles.Interfaces;
using SmartInsure.Application.UseCase.UseCases.ProfileUseCases.ListProfiles.Requests;
using SmartInsure.Application.UseCase.UseCases.ProfileUseCases.ListProfiles.Responses;
using SmartInsure.Application.UseCase.UseCases.ProfileUseCases.UpdateFixedProfilePermissions.Interfaces;
using SmartInsure.Application.UseCase.UseCases.ProfileUseCases.UpdateFixedProfilePermissions.Requests;
using SmartInsure.Application.UseCase.UseCases.ProfileUseCases.UpdateFixedProfilePermissions.Responses;
using SmartInsure.Application.UseCase.UseCases.ProfileUseCases.UpdateScopedProfile.Interfaces;
using SmartInsure.Application.UseCase.UseCases.ProfileUseCases.UpdateScopedProfile.Requests;
using SmartInsure.Application.UseCase.UseCases.ProfileUseCases.UpdateScopedProfile.Responses;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Constants;

namespace SmartInsure.Api.Endpoints;

/// <summary>
/// Jornada Perfis e Permissões: gestão dos Perfis (RN-062) e das Permissões marcadas (RN-063).
/// A visibilidade e a autorização são por Escopo (RN-072/RN-074) — o Administrador do Sistema vê
/// tudo, o administrador de Escopo só o próprio —, então a decisão fica no use case e não em
/// policy de rota. Só a edição das Permissões de Perfil fixo é exclusiva do Administrador do
/// Sistema (RN-073), e essa ainda não existe.
/// </summary>
public sealed class ProfilesEndpoint : CarterModule
{
    public ProfilesEndpoint()
        : base("profiles")
    {
    }

    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/", ListAsync)
            .RequireAuthorization()
            .Produces<PagedResponse<ProfileListItemResponse>>(StatusCodes.Status200OK);

        // RN-072: Perfis que o próprio solicitante pode atribuir no Escopo ativo — a rota vem antes
        // de "/{id:guid}" porque "assignable" não é um GUID e cairia no outro padrão.
        app.MapGet("/assignable", ListAssignableAsync)
            .RequireAuthorization()
            .Produces<IReadOnlyList<AssignableProfileResponse>>(StatusCodes.Status200OK);

        app.MapGet("/{id:guid}", GetAsync)
            .RequireAuthorization()
            .Produces<GetProfileResponse>(StatusCodes.Status200OK);

        // RN-069/RN-070: Perfil customizado do Escopo administrado pelo solicitante.
        app.MapPost("/", CreateAsync)
            .RequireAuthorization()
            .Produces<CreateScopedProfileResponse>(StatusCodes.Status201Created);

        // RN-074: edição e remoção ficam com o administrador do Escopo do Perfil.
        app.MapPut("/{id:guid}", UpdateAsync)
            .RequireAuthorization()
            .Produces<UpdateScopedProfileResponse>(StatusCodes.Status200OK);

        app.MapDelete("/{id:guid}", DeleteAsync)
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent);

        // RN-073: só o Administrador do Sistema marca/desmarca Permissões de Perfil fixo, e o
        // efeito é global — aqui a autorização é policy de rota (Perfil de Escopo Sistema).
        app.MapPut("/{id:guid}/permissions", UpdateFixedPermissionsAsync)
            .RequireAuthorization(Policies.SystemAdministrator)
            .Produces<UpdateFixedProfilePermissionsResponse>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> UpdateFixedPermissionsAsync(
        HttpContext httpContext,
        RequestHandler handler,
        IUpdateFixedProfilePermissionsUseCase useCase,
        Guid id,
        FixedProfilePermissionsBody body)
        => await handler.TryHandleAsync(
            httpContext,
            useCase,
            new UpdateFixedProfilePermissionsRequest(id, body.PermissionCodes ?? []));

    private static async Task<IResult> ListAsync(
        HttpContext httpContext,
        RequestHandler handler,
        IListProfilesUseCase useCase,
        ICurrentUserAccessor currentUser,
        int? page,
        int? pageSize,
        string? search,
        string? scope)
        => await handler.TryHandleAsync(
            httpContext,
            useCase,
            new ListProfilesRequest
            {
                ExternalIdentity = currentUser.UserIdentifier ?? string.Empty,
                ActiveBrokerageId = currentUser.ActiveBrokerageId,
                ActivePolicyHolderId = currentUser.ActivePolicyHolderId,
                Page = page ?? 1,
                PageSize = pageSize ?? 20,
                Search = search,
                Scope = scope,
            });

    private static async Task<IResult> GetAsync(
        HttpContext httpContext,
        RequestHandler handler,
        IGetProfileUseCase useCase,
        ICurrentUserAccessor currentUser,
        Guid id)
        => await handler.TryHandleAsync(
            httpContext,
            useCase,
            new GetProfileRequest(
                id,
                currentUser.UserIdentifier ?? string.Empty,
                currentUser.ActiveBrokerageId,
                currentUser.ActivePolicyHolderId));

    private static async Task<IResult> ListAssignableAsync(
        HttpContext httpContext,
        RequestHandler handler,
        IListAssignableProfilesUseCase useCase,
        ICurrentUserAccessor currentUser)
        => await handler.TryHandleAsync(
            httpContext,
            useCase,
            new ListAssignableProfilesRequest(
                currentUser.UserIdentifier ?? string.Empty,
                currentUser.ActiveBrokerageId,
                currentUser.ActivePolicyHolderId));

    private static async Task<IResult> CreateAsync(
        HttpContext httpContext,
        RequestHandler handler,
        ICreateScopedProfileUseCase useCase,
        IValidator<CreateScopedProfileRequest> validator,
        ICurrentUserAccessor currentUser,
        ScopedProfileBody body)
        => await handler.TryHandleAsync(
            httpContext,
            useCase,
            new CreateScopedProfileRequest(
                currentUser.UserIdentifier ?? string.Empty,
                currentUser.ActiveBrokerageId,
                currentUser.ActivePolicyHolderId,
                body.Name,
                body.PermissionCodes ?? []),
            validator,
            response => Results.Created($"/api/v1/profiles/{response.Id}", response));

    private static async Task<IResult> UpdateAsync(
        HttpContext httpContext,
        RequestHandler handler,
        IUpdateScopedProfileUseCase useCase,
        IValidator<UpdateScopedProfileRequest> validator,
        ICurrentUserAccessor currentUser,
        Guid id,
        ScopedProfileBody body)
        => await handler.TryHandleAsync(
            httpContext,
            useCase,
            new UpdateScopedProfileRequest(
                currentUser.UserIdentifier ?? string.Empty,
                currentUser.ActiveBrokerageId,
                currentUser.ActivePolicyHolderId,
                id,
                body.Name,
                body.PermissionCodes ?? []),
            validator);

    private static async Task<IResult> DeleteAsync(
        HttpContext httpContext,
        RequestHandler handler,
        IDeleteScopedProfileUseCase useCase,
        ICurrentUserAccessor currentUser,
        Guid id)
        => await handler.TryHandleAsync(
            httpContext,
            useCase,
            new DeleteScopedProfileRequest(
                currentUser.UserIdentifier ?? string.Empty,
                currentUser.ActiveBrokerageId,
                currentUser.ActivePolicyHolderId,
                id),
            resultFactory: _ => Results.NoContent());
}

/// <summary>
/// RN-069/RN-070/RN-074: corpo de criação e edição de Perfil customizado. O Escopo não vem daqui —
/// é o Escopo ativo do solicitante, lido do acesso (SECURITY.md).
/// </summary>
public sealed record ScopedProfileBody(string Name, IReadOnlyCollection<string>? PermissionCodes);

/// <summary>
/// RN-073: corpo da edição de Permissões de Perfil fixo. Só as Permissões — nome e Escopo do
/// Perfil fixo são imutáveis.
/// </summary>
public sealed record FixedProfilePermissionsBody(IReadOnlyCollection<string>? PermissionCodes);

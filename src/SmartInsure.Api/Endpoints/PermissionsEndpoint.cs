using Carter;
using SmartInsure.Api.Handlers.Base;
using SmartInsure.Application.UseCase.UseCases.PermissionUseCases.ListPermissions.Interfaces;
using SmartInsure.Application.UseCase.UseCases.PermissionUseCases.ListPermissions.Requests;
using SmartInsure.Application.UseCase.UseCases.PermissionUseCases.ListPermissions.Responses;

namespace SmartInsure.Api.Endpoints;

/// <summary>
/// Catálogo fixo de Permissões (RN-063). Só leitura: o catálogo é declarado pela plataforma e
/// semeado por migration — ninguém cria Permissão por tela. Qualquer Usuário autenticado consulta,
/// porque é a lista oferecida ao marcar Permissões num Perfil que ele administra.
/// </summary>
public sealed class PermissionsEndpoint : CarterModule
{
    public PermissionsEndpoint()
        : base("permissions")
    {
    }

    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/", ListAsync)
            .RequireAuthorization()
            .Produces<IReadOnlyList<PermissionResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> ListAsync(
        HttpContext httpContext,
        RequestHandler handler,
        IListPermissionsUseCase useCase)
        => await handler.TryHandleAsync(httpContext, useCase, new ListPermissionsRequest());
}

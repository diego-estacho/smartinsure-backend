using Carter;
using FluentValidation;
using SmartInsure.Api.Handlers.Base;
using SmartInsure.Application.UseCase.ModelsBase;
using SmartInsure.Application.UseCase.UseCases.InsurerUseCases.ChangeInsurerStatus.Interfaces;
using SmartInsure.Application.UseCase.UseCases.InsurerUseCases.ChangeInsurerStatus.Requests;
using SmartInsure.Application.UseCase.UseCases.InsurerUseCases.ChangeInsurerStatus.Responses;
using SmartInsure.Application.UseCase.UseCases.InsurerUseCases.CreateInsurer.Interfaces;
using SmartInsure.Application.UseCase.UseCases.InsurerUseCases.CreateInsurer.Requests;
using SmartInsure.Application.UseCase.UseCases.InsurerUseCases.CreateInsurer.Responses;
using SmartInsure.Application.UseCase.UseCases.InsurerUseCases.GetInsurer.Interfaces;
using SmartInsure.Application.UseCase.UseCases.InsurerUseCases.GetInsurer.Requests;
using SmartInsure.Application.UseCase.UseCases.InsurerUseCases.GetInsurer.Responses;
using SmartInsure.Application.UseCase.UseCases.InsurerUseCases.ListInsurers.Interfaces;
using SmartInsure.Application.UseCase.UseCases.InsurerUseCases.ListInsurers.Requests;
using SmartInsure.Application.UseCase.UseCases.InsurerUseCases.ListInsurers.Responses;
using SmartInsure.Application.UseCase.UseCases.InsurerUseCases.UpdateInsurer.Interfaces;
using SmartInsure.Application.UseCase.UseCases.InsurerUseCases.UpdateInsurer.Requests;
using SmartInsure.Application.UseCase.UseCases.InsurerUseCases.UpdateInsurer.Responses;
using SmartInsure.Core.Constants;

namespace SmartInsure.Api.Endpoints;

/// <summary>
/// Jornada Seguradoras (RN-007..RN-011): escrita exclusiva do Administrador do Sistema
/// (policy fail-closed); leitura para qualquer autenticado.
/// </summary>
public sealed class InsurersEndpoint : CarterModule
{
    public InsurersEndpoint()
        : base("insurers")
    {
    }

    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/", CreateAsync)
            .RequireAuthorization(Policies.SystemAdministrator)
            .Produces<CreateInsurerResponse>(StatusCodes.Status201Created);

        app.MapPut("/{id:guid}", UpdateAsync)
            .RequireAuthorization(Policies.SystemAdministrator)
            .Produces<UpdateInsurerResponse>(StatusCodes.Status200OK);

        app.MapPatch("/{id:guid}/status", ChangeStatusAsync)
            .RequireAuthorization(Policies.SystemAdministrator)
            .Produces<ChangeInsurerStatusResponse>(StatusCodes.Status200OK);

        app.MapGet("/", ListAsync)
            .Produces<PagedResponse<InsurerListItemResponse>>(StatusCodes.Status200OK);

        app.MapGet("/{id:guid}", GetAsync)
            .Produces<GetInsurerResponse>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> CreateAsync(
        HttpContext httpContext,
        RequestHandler handler,
        ICreateInsurerUseCase useCase,
        IValidator<CreateInsurerRequest> validator,
        CreateInsurerRequest request)
        => await handler.TryHandleAsync(
            httpContext,
            useCase,
            request,
            validator,
            response => Results.Created($"/api/v1/insurers/{response.Id}", response));

    private static async Task<IResult> UpdateAsync(
        HttpContext httpContext,
        RequestHandler handler,
        IUpdateInsurerUseCase useCase,
        IValidator<UpdateInsurerRequest> validator,
        Guid id,
        UpdateInsurerBody body)
        => await handler.TryHandleAsync(
            httpContext,
            useCase,
            new UpdateInsurerRequest(id, body.Cnpj, body.CorporateName, body.TradeName, body.LogoUrl),
            validator);

    private static async Task<IResult> ChangeStatusAsync(
        HttpContext httpContext,
        RequestHandler handler,
        IChangeInsurerStatusUseCase useCase,
        IValidator<ChangeInsurerStatusRequest> validator,
        Guid id,
        ChangeInsurerStatusBody body)
        => await handler.TryHandleAsync(
            httpContext,
            useCase,
            new ChangeInsurerStatusRequest(id, body.Status),
            validator);

    /// <summary>RN-010: visão completa só surte efeito para o Administrador do Sistema.</summary>
    private static async Task<IResult> ListAsync(
        HttpContext httpContext,
        RequestHandler handler,
        IListInsurersUseCase useCase,
        int? page,
        int? pageSize,
        bool? includeInactive)
        => await handler.TryHandleAsync(
            httpContext,
            useCase,
            new ListInsurersRequest
            {
                Page = page ?? 1,
                PageSize = pageSize ?? 20,
                IncludeInactive = includeInactive ?? false,
                CallerIsSystemAdministrator = httpContext.User.IsInRole(Roles.SystemAdministrator),
            });

    private static async Task<IResult> GetAsync(
        HttpContext httpContext,
        RequestHandler handler,
        IGetInsurerUseCase useCase,
        Guid id)
        => await handler.TryHandleAsync(httpContext, useCase, new GetInsurerRequest(id));
}

/// <summary>Corpo do PUT — o id vem da rota.</summary>
public sealed record UpdateInsurerBody(
    string Cnpj, string CorporateName, string? TradeName, string? LogoUrl);

/// <summary>Corpo do PATCH de situação — nome estável Active/Inactive.</summary>
public sealed record ChangeInsurerStatusBody(string Status);

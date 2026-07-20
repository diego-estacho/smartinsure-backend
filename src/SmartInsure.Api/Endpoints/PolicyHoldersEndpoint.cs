using Carter;
using FluentValidation;
using SmartInsure.Api.Handlers.Base;
using SmartInsure.Application.UseCase.ModelsBase;
using SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.AddPolicyHolderAddress.Interfaces;
using SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.AddPolicyHolderAddress.Requests;
using SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.CreatePolicyHolder.Interfaces;
using SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.CreatePolicyHolder.Requests;
using SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.CreatePolicyHolder.Responses;
using SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.CreatePolicyHolderAppointment.Interfaces;
using SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.CreatePolicyHolderAppointment.Requests;
using SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.CreatePolicyHolderAppointment.Responses;
using SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.EndPolicyHolderAppointment.Interfaces;
using SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.EndPolicyHolderAppointment.Requests;
using SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.GetPolicyHolder.Interfaces;
using SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.GetPolicyHolder.Requests;
using SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.GetPolicyHolder.Responses;
using SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.ListPolicyHolders.Interfaces;
using SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.ListPolicyHolders.Requests;
using SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.ListPolicyHolders.Responses;
using SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.RemovePolicyHolderAddress.Interfaces;
using SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.RemovePolicyHolderAddress.Requests;
using SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.UpdatePolicyHolderAddress.Interfaces;
using SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.UpdatePolicyHolderAddress.Requests;

namespace SmartInsure.Api.Endpoints;

/// <summary>
/// Jornada Tomadores (RN-025..RN-028): qualquer Usuário autenticado lista, cria,
/// detalha e gerencia endereços e nomeações de Tomadores nesta fase.
/// </summary>
public sealed class PolicyHoldersEndpoint : CarterModule
{
    public PolicyHoldersEndpoint()
        : base("policy-holders")
    {
    }

    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/", ListAsync)
            .Produces<PagedResponse<PolicyHolderListItemResponse>>(StatusCodes.Status200OK);

        app.MapGet("/{id:guid}", GetAsync)
            .Produces<GetPolicyHolderResponse>(StatusCodes.Status200OK);

        app.MapPost("/", CreateAsync)
            .Produces<CreatePolicyHolderResponse>(StatusCodes.Status201Created);

        app.MapPost("/{id:guid}/addresses", AddAddressAsync)
            .Produces(StatusCodes.Status204NoContent);

        app.MapPut("/{id:guid}/addresses/{addressId:guid}", UpdateAddressAsync)
            .Produces(StatusCodes.Status204NoContent);

        app.MapDelete("/{id:guid}/addresses/{addressId:guid}", RemoveAddressAsync)
            .Produces(StatusCodes.Status204NoContent);

        app.MapPost("/{id:guid}/appointments", CreateAppointmentAsync)
            .Produces<CreatePolicyHolderAppointmentResponse>(StatusCodes.Status201Created);

        app.MapPatch("/{id:guid}/appointments/{appointmentId:guid}/end", EndAppointmentAsync)
            .Produces(StatusCodes.Status204NoContent);
    }

    private static async Task<IResult> ListAsync(
        HttpContext httpContext,
        RequestHandler handler,
        IListPolicyHoldersUseCase useCase,
        int? page,
        int? pageSize,
        string? search)
        => await handler.TryHandleAsync(
            httpContext,
            useCase,
            new ListPolicyHoldersRequest
            {
                Page = page ?? 1,
                PageSize = pageSize ?? 20,
                Search = search,
            });

    private static async Task<IResult> GetAsync(
        HttpContext httpContext,
        RequestHandler handler,
        IGetPolicyHolderUseCase useCase,
        Guid id)
        => await handler.TryHandleAsync(httpContext, useCase, new GetPolicyHolderRequest(id));

    private static async Task<IResult> CreateAsync(
        HttpContext httpContext,
        RequestHandler handler,
        ICreatePolicyHolderUseCase useCase,
        IValidator<CreatePolicyHolderRequest> validator,
        CreatePolicyHolderRequest request)
        => await handler.TryHandleAsync(
            httpContext,
            useCase,
            request,
            validator,
            response => Results.Created($"/api/v1/policy-holders/{response.Id}", response));

    private static async Task<IResult> AddAddressAsync(
        HttpContext httpContext,
        RequestHandler handler,
        IAddPolicyHolderAddressUseCase useCase,
        IValidator<AddPolicyHolderAddressRequest> validator,
        Guid id,
        AddPolicyHolderAddressBody body)
        => await handler.TryHandleAsync(
            httpContext,
            useCase,
            new AddPolicyHolderAddressRequest(id, body.ZipCode, body.Street, body.Number,
                body.Complement, body.Neighborhood, body.City, body.State),
            validator,
            _ => Results.NoContent());

    private static async Task<IResult> UpdateAddressAsync(
        HttpContext httpContext,
        RequestHandler handler,
        IUpdatePolicyHolderAddressUseCase useCase,
        IValidator<UpdatePolicyHolderAddressRequest> validator,
        Guid id,
        Guid addressId,
        UpdatePolicyHolderAddressBody body)
        => await handler.TryHandleAsync(
            httpContext,
            useCase,
            new UpdatePolicyHolderAddressRequest(id, addressId, body.ZipCode, body.Street,
                body.Number, body.Complement, body.Neighborhood, body.City, body.State),
            validator,
            _ => Results.NoContent());

    private static async Task<IResult> RemoveAddressAsync(
        HttpContext httpContext,
        RequestHandler handler,
        IRemovePolicyHolderAddressUseCase useCase,
        Guid id,
        Guid addressId)
        => await handler.TryHandleAsync(
            httpContext,
            useCase,
            new RemovePolicyHolderAddressRequest(id, addressId),
            null,
            _ => Results.NoContent());

    private static async Task<IResult> CreateAppointmentAsync(
        HttpContext httpContext,
        RequestHandler handler,
        ICreatePolicyHolderAppointmentUseCase useCase,
        IValidator<CreatePolicyHolderAppointmentRequest> validator,
        Guid id,
        CreatePolicyHolderAppointmentBody body)
        => await handler.TryHandleAsync(
            httpContext,
            useCase,
            new CreatePolicyHolderAppointmentRequest(id, body.BrokerageId, body.InsurerId),
            validator,
            response => Results.Created($"/api/v1/policy-holders/{id}/appointments/{response.Id}", response));

    private static async Task<IResult> EndAppointmentAsync(
        HttpContext httpContext,
        RequestHandler handler,
        IEndPolicyHolderAppointmentUseCase useCase,
        Guid id,
        Guid appointmentId)
        => await handler.TryHandleAsync(
            httpContext,
            useCase,
            new EndPolicyHolderAppointmentRequest(appointmentId),
            null,
            _ => Results.NoContent());
}

public sealed record AddPolicyHolderAddressBody(
    string? ZipCode,
    string? Street,
    string? Number,
    string? Complement,
    string? Neighborhood,
    string? City,
    string? State);

public sealed record UpdatePolicyHolderAddressBody(
    string? ZipCode,
    string? Street,
    string? Number,
    string? Complement,
    string? Neighborhood,
    string? City,
    string? State);

public sealed record CreatePolicyHolderAppointmentBody(Guid BrokerageId, Guid InsurerId);

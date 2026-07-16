using Carter;
using FluentValidation;
using SmartInsure.Api.Handlers.Base;
using SmartInsure.Application.UseCase.UseCases.PersonUseCases.SearchPersons.Interfaces;
using SmartInsure.Application.UseCase.UseCases.PersonUseCases.SearchPersons.Requests;
using SmartInsure.Application.UseCase.UseCases.PersonUseCases.SearchPersons.Responses;

namespace SmartInsure.Api.Endpoints;

/// <summary>
/// Jornada Cadastro de Pessoas (RN-013..RN-016): busca de Pessoa por nome ou
/// documento, com importação do Birô quando o CNPJ não está na base.
/// </summary>
public sealed class PersonsEndpoint : CarterModule
{
    public PersonsEndpoint()
        : base("persons")
    {
    }

    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/", SearchAsync)
            .Produces<SearchPersonsResponse>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> SearchAsync(
        HttpContext httpContext,
        RequestHandler handler,
        ISearchPersonsUseCase useCase,
        IValidator<SearchPersonsRequest> validator,
        string? term,
        string? role)
        => await handler.TryHandleAsync(
            httpContext,
            useCase,
            new SearchPersonsRequest(term ?? string.Empty, role ?? string.Empty),
            validator);
}

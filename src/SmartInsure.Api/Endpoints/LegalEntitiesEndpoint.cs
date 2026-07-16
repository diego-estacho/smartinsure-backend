using Carter;
using FluentValidation;
using SmartInsure.Api.Handlers.Base;
using SmartInsure.Application.UseCase.UseCases.LegalEntityUseCases.SearchLegalEntities.Interfaces;
using SmartInsure.Application.UseCase.UseCases.LegalEntityUseCases.SearchLegalEntities.Requests;
using SmartInsure.Application.UseCase.UseCases.LegalEntityUseCases.SearchLegalEntities.Responses;

namespace SmartInsure.Api.Endpoints;

/// <summary>
/// Jornada Cadastro de Pessoas (RN-013..RN-016): busca de Pessoa Jurídica por nome ou
/// documento, com importação do Birô quando o CNPJ não está na base.
/// </summary>
public sealed class LegalEntitiesEndpoint : CarterModule
{
    public LegalEntitiesEndpoint()
        : base("legal-entities")
    {
    }

    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/", SearchAsync)
            .Produces<SearchLegalEntitiesResponse>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> SearchAsync(
        HttpContext httpContext,
        RequestHandler handler,
        ISearchLegalEntitiesUseCase useCase,
        IValidator<SearchLegalEntitiesRequest> validator,
        string? term,
        string? role)
        => await handler.TryHandleAsync(
            httpContext,
            useCase,
            new SearchLegalEntitiesRequest(term ?? string.Empty, role ?? string.Empty),
            validator);
}

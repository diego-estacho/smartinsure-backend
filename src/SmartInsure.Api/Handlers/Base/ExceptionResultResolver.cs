using SmartInsure.Core.Exceptions;

namespace SmartInsure.Api.Handlers.Base;

/// <summary>
/// Resolver central de exceções (ADR-011): mapa 1:1 exceção tipada → ProblemDetails
/// (ADR-012, ADR-022). Detalhe interno de exceção nunca aparece na resposta.
/// </summary>
public sealed class ExceptionResultResolver(
    ProblemResultFactory problemFactory,
    ILogger<ExceptionResultResolver> logger)
{
    public IResult Resolve(HttpContext httpContext, Exception exception) => exception switch
    {
        NotFoundException notFound => problemFactory.Problem(
            httpContext, StatusCodes.Status404NotFound, "Recurso não encontrado.", notFound.Message),

        ConflictException conflict => problemFactory.Problem(
            httpContext, StatusCodes.Status409Conflict, "Conflito de estado.", conflict.Message),

        BusinessRuleException businessRule => problemFactory.Problem(
            httpContext, StatusCodes.Status422UnprocessableEntity, "Regra de negócio impede a operação.", businessRule.Message),

        _ => Unexpected(httpContext, exception),
    };

    private IResult Unexpected(HttpContext httpContext, Exception exception)
    {
        logger.LogError(exception, "Erro inesperado no pipeline de request");

        return problemFactory.Problem(
            httpContext,
            StatusCodes.Status500InternalServerError,
            "Erro inesperado.",
            "Ocorreu um erro inesperado. Tente novamente ou contate o suporte informando o correlationId.");
    }
}

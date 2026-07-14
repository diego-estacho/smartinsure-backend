using System.Diagnostics;
using SmartInsure.Api.Extensions;

namespace SmartInsure.Api.Handlers.Base;

/// <summary>
/// Fábrica única de ProblemDetails (RFC 9457) enriquecidos com traceId e correlationId
/// (ADR-012, ADR-055). Endpoints nunca constroem respostas de erro à mão.
/// </summary>
public sealed class ProblemResultFactory
{
    public IResult Problem(HttpContext httpContext, int statusCode, string title, string detail)
        => Results.Problem(
            detail: detail,
            statusCode: statusCode,
            title: title,
            extensions: Enrich(httpContext));

    public IResult ValidationProblem(HttpContext httpContext, IDictionary<string, string[]> errors)
        => Results.ValidationProblem(
            errors,
            title: "Falha de validação do request.",
            extensions: Enrich(httpContext));

    private static Dictionary<string, object?> Enrich(HttpContext httpContext) => new()
    {
        ["traceId"] = Activity.Current?.TraceId.ToString() ?? httpContext.TraceIdentifier,
        ["correlationId"] = httpContext.Items[CorrelationIdMiddleware.ItemKey]?.ToString(),
    };
}

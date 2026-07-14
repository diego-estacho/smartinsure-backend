namespace SmartInsure.Api.Extensions;

/// <summary>
/// Propagação de CorrelationId (ADR-055): recebido do gateway ou gerado na borda,
/// disponível no contexto do request e devolvido no header de resposta.
/// </summary>
public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    public const string HeaderName = "X-Correlation-Id";
    public const string ItemKey = "CorrelationId";

    public async Task InvokeAsync(HttpContext httpContext)
    {
        var correlationId = httpContext.Request.Headers.TryGetValue(HeaderName, out var received)
            && !string.IsNullOrWhiteSpace(received)
                ? received.ToString()
                : Guid.CreateVersion7().ToString();

        httpContext.Items[ItemKey] = correlationId;
        httpContext.Response.Headers[HeaderName] = correlationId;

        await next(httpContext);
    }
}

public static class CorrelationIdExtensions
{
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
        => app.UseMiddleware<CorrelationIdMiddleware>();
}

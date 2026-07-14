using FluentValidation;
using SmartInsure.Application.UseCase.Common;

namespace SmartInsure.Api.Handlers.Base;

/// <summary>
/// Pipeline central de request (ADR-011): validação FluentValidation → execução do
/// UseCase → mapeamento de exceções → resposta. Todo handler de endpoint delega aqui;
/// try/catch em endpoint nunca é escrito.
/// </summary>
public sealed class RequestHandler(
    ExceptionResultResolver exceptionResolver,
    ProblemResultFactory problemFactory)
{
    public async Task<IResult> TryHandleAsync<TRequest, TResponse>(
        HttpContext httpContext,
        IUseCase<TRequest, TResponse> useCase,
        TRequest request,
        IValidator<TRequest>? validator = null,
        Func<TResponse, IResult>? resultFactory = null)
    {
        var cancellationToken = httpContext.RequestAborted;

        try
        {
            if (validator is not null)
            {
                var validation = await validator.ValidateAsync(request, cancellationToken);

                if (!validation.IsValid)
                {
                    return problemFactory.ValidationProblem(httpContext, validation.ToDictionary());
                }
            }

            var response = await useCase.ExecuteAsync(request, cancellationToken);

            return resultFactory?.Invoke(response) ?? Results.Ok(response);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            return exceptionResolver.Resolve(httpContext, exception);
        }
    }
}

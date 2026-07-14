namespace SmartInsure.Application.UseCase.Common;

/// <summary>
/// Contrato base de UseCase (ADR-020): uma ação de negócio por implementação.
/// Erro de negócio é sinalizado exclusivamente por exceção tipada do Core (ADR-022).
/// Ações sem retorno usam <see cref="Unit"/>.
/// </summary>
public interface IUseCase<in TRequest, TResponse>
{
    Task<TResponse> ExecuteAsync(TRequest request, CancellationToken cancellationToken);
}

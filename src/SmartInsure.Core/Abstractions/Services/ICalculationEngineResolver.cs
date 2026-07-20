using SmartInsure.Core.Abstractions.Services.Dtos;

namespace SmartInsure.Core.Abstractions.Services;

/// <summary>
/// Resolvedor do Motor de Cálculo (RN-023): dado o par Corretora×Seguradora, devolve o
/// motor configurado na Habilitação Ativa com os parâmetros de conexão do vínculo.
/// Sem Habilitação, Habilitação Inativa, Seguradora Inativa (RN-010) ou motor
/// indisponível: a operação é recusada com exceção de regra de negócio.
/// </summary>
public interface ICalculationEngineResolver
{
    Task<CalculationEngineResolution> ResolveAsync(
        Guid brokerageId,
        Guid insurerId,
        CancellationToken cancellationToken);
}

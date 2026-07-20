using SmartInsure.Core.Enumerators;

namespace SmartInsure.Core.Abstractions.Services;

/// <summary>
/// Contrato do Motor de Cálculo (glossário) — implementado na camada de integração.
/// RN-023: nenhuma operação junto à seguradora escolhe motor fixo no código; a instância
/// chega sempre pelo resolvedor, configurada pela Habilitação de Seguradora.
/// As operações do motor (cotar, prêmio, dados de apoio, emissão, cancelamento) entram
/// neste contrato nas demandas de cada jornada (OPEN-07).
/// </summary>
public interface ICalculationEngine
{
    ECalculationEngine Engine { get; }

    /// <summary>
    /// RN-022 (caso limite): parâmetros de conexão obrigatórios do motor ausentes ou
    /// inválidos recusam a gravação da Habilitação. Lança exceção de regra de negócio.
    /// </summary>
    void EnsureValidConnectionParameters(string? connectionParameters);
}

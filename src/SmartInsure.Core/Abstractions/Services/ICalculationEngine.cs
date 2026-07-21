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

    /// <summary>
    /// RN-029: consulta os Limites de Crédito de um tomador junto à Seguradora.
    /// Retorna limites e taxas por modalidade (Tradicional/Judicial/Financeiro) com validade,
    /// ou null se indisponível. Exceções são do tipo CalculationEngineException.
    /// </summary>
    Task<PolicyHolderLimitsAndRates?> GetPolicyHolderLimitsAndRatesAsync(
        string? connectionParameters,
        string brokerageCnpj,
        string policyHolderCnpj,
        string insurerExternalId,
        CancellationToken cancellationToken);
}

/// <summary>
/// Resposta da consulta de limites de crédito por modalidade (RN-029).
/// Campos são opcionais — seguradora pode retornar somente algumas modalidades.
/// </summary>
public sealed record PolicyHolderLimitsAndRates
{
    public decimal? TraditionalLimit { get; init; }
    public decimal? TraditionalRate { get; init; }

    public decimal? JudicialLimit { get; init; }
    public decimal? JudicialRate { get; init; }
    public decimal? JudicialFiscalRate { get; init; }

    public decimal? FinancialLimit { get; init; }
    public decimal? FinancialRate { get; init; }

    public DateTime? LimitValidUntil { get; init; }
}

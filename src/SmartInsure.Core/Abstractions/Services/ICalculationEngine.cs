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
    /// Retorna limites e taxas agrupados por grupo de modalidade (dinâmicos conforme retorno da Seguradora),
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
/// Resposta da consulta de limites de crédito agrupados por grupo de modalidade (RN-029).
/// Cada grupo contém o maior limite disponível entre as modalidades que o compõem.
/// </summary>
public sealed record PolicyHolderLimitsAndRates
{
    /// <summary>Razão social do tomador, quando informada pela Seguradora.</summary>
    public string? PolicyHolderName { get; init; }

    /// <summary>Grupos de modalidade com limites e taxas (ex.: Tradicional, Judicial, Financeira).</summary>
    public required IReadOnlyList<PolicyHolderLimitGroup> Groups { get; init; }
}

/// <summary>
/// Grupo de modalidades com limites agregados (RN-029).
/// Valor do grupo = maior AvailableLimit entre modalidades que o compõem.
/// </summary>
public sealed record PolicyHolderLimitGroup
{
    /// <summary>Nome do grupo (ex.: "Tradicional", "Judiciais", "Financeira").</summary>
    public required string GroupName { get; init; }

    /// <summary>Tipo do grupo (ex.: "GARANTIA_TRADICIONAL").</summary>
    public required string GroupType { get; init; }

    /// <summary>Limite disponível — maior AvailableLimit do grupo.</summary>
    public required decimal AvailableLimit { get; init; }

    /// <summary>Limite revisado — maior LimitRevised do grupo.</summary>
    public required decimal RevisedLimit { get; init; }

    /// <summary>Taxa — da modalidade com maior AvailableLimit do grupo.</summary>
    public required decimal Rate { get; init; }
}

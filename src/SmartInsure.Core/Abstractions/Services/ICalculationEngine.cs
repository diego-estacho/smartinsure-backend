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
    /// RN-034: obtém o catálogo de modalidades das Seguradoras habilitadas da Corretora,
    /// usando os parâmetros de conexão da Habilitação e o CNPJ da Corretora. A tradução do
    /// payload do fornecedor para o contrato acontece na ACL do provider (ADR-045).
    /// </summary>
    Task<ImportedCatalogResult> GetGroupAndModalitiesAsync(
        string? connectionParameters, string brokerCnpj, CancellationToken cancellationToken);

    /// <summary>
    /// RN-042/RN-044: obtém as Coberturas Adicionais de UMA Modalidade Importada, identificada pelo
    /// nome de origem e pelo tipo do grupo, junto à Seguradora (InsuranceUniqueId), usando os
    /// parâmetros de conexão da Habilitação e o CNPJ da Corretora. A tradução do payload do
    /// fornecedor para o contrato acontece na ACL do provider (ADR-045).
    /// </summary>
    Task<ImportedAdditionalCoverageResult> GetAdditionalCoveragesAsync(
        string? connectionParameters,
        string brokerCnpj,
        string insuranceUniqueId,
        string modalityName,
        string? modalityGroupType,
        CancellationToken cancellationToken);

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

    /// <summary>
    /// RN-047/048: obtém o objeto de uma modalidade (Tag + Cláusulas particulares) na OnPoint,
    /// por ModalityUniqueId. HasError=true (ou envelope inválido) sinaliza falha isolada da
    /// modalidade (RN-049); falha de transporte sobe como exceção. Tradução na ACL (ADR-045).
    /// </summary>
    Task<ModalityObjectResult> GetModalityObjectAsync(
        string? connectionParameters, string brokerCnpj, string modalityUniqueId, CancellationToken cancellationToken);
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

/// <summary>Objeto da modalidade (RN-047/048): a Tag e as Cláusulas particulares vindas no mesmo payload.</summary>
public sealed record ModalityObjectResult(
    bool HasError, string? JsonTag, string? ObjectText, IReadOnlyList<ModalityClauseData> Clauses);

/// <summary>Cláusula particular como recebida da fonte (RN-048).</summary>
public sealed record ModalityClauseData(string ExternalId, string Name, string? Text, string? JsonTag);

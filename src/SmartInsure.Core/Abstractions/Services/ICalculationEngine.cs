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

    /// <summary>
    /// RN-057/RN-058: solicita uma Cotação a UMA Seguradora (POST /Cotation no PlugV2), que cria a
    /// proposta e roda a esteira, devolvendo o resultado já classificado pela ACL (ADR-064). Falha de
    /// transporte/desserialização sobe como CalculationEngineException — o consumidor registra a Cotação
    /// como falha (RN-057, sem retry automático).
    /// </summary>
    Task<QuotationResult> RunQuotationAsync(
        string? connectionParameters, QuotationRequestInput request, CancellationToken cancellationToken);

    /// <summary>
    /// RN-063 ("Baixar minuta", parte 1): envia ao provedor os termos preenchidos da proposta selecionada
    /// — as Tags do objeto e as Cláusulas particulares marcadas (POST /UpdateProposalTerms). Preenchimento
    /// parcial é aceito. Falha de transporte sobe como CalculationEngineException.
    /// </summary>
    Task SubmitProposalTermsAsync(
        string? connectionParameters, SubmitProposalTermsInput request, CancellationToken cancellationToken);

    /// <summary>
    /// RN-063 ("Baixar minuta", parte 2): obtém a minuta (documento) da proposta no provedor
    /// (POST /GetProposalContractDraft), tipicamente uma URL para o contrato gerado. Falha de transporte
    /// sobe como CalculationEngineException.
    /// </summary>
    Task<ProposalContractDraftResult> GetProposalContractDraftAsync(
        string? connectionParameters, string brokerCnpj, string proposalExternalId, CancellationToken cancellationToken);
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

/// <summary>
/// Dados de entrada para solicitar uma Cotação a UMA Seguradora (RN-056/RN-057) — o risco do Grupo de
/// Cotação. Os parâmetros de conexão (baseUrl/key) vêm à parte, da Habilitação (RN-023).
/// </summary>
public sealed record QuotationRequestInput
{
    /// <summary>Identificadores do fan-out (ADR-050/ADR-102) — carregados só para o log de integração (QuotationIntegrationLog); a chamada ao provedor não os usa.</summary>
    public required Guid QuotationId { get; init; }

    public required Guid QuotationGroupId { get; init; }

    public required Guid InsurerId { get; init; }

    public required string BrokerCnpj { get; init; }

    public required string PolicyHolderCnpj { get; init; }

    public required string InsuredCpfCnpj { get; init; }

    public required string InsuranceUniqueId { get; init; }

    public required string ModalityGlobalId { get; init; }

    public string? ModalityName { get; init; }

    public required decimal InsuredAmount { get; init; }

    public required DateOnly StartDate { get; init; }

    public required DateOnly EndDate { get; init; }

    public IReadOnlyList<string> AdditionalCoverages { get; init; } = [];
}

/// <summary>
/// Resultado da Cotação já traduzido pela ACL do motor (ADR-064): classificação estável + esteira +
/// motivos + prêmio/limite/CCG. Prêmio só vem em Automatic; Analysis traz esteira; Unavailable traz
/// motivos; Unrecognized vem sem prêmio/esteira. CCG é ortogonal à classificação.
/// </summary>
public sealed record QuotationResult
{
    public required EQuotationResult Result { get; init; }

    public EAnalysisTrack? AnalysisTrack { get; init; }

    public decimal? Premium { get; init; }

    public decimal? CommissionPercentage { get; init; }

    public decimal? CommissionValue { get; init; }

    public decimal? Tax { get; init; }

    public decimal? AvailableLimit { get; init; }

    public string? ProposalExternalId { get; init; }

    public string? ProposalNumber { get; init; }

    public bool RequiresCcg { get; init; }

    public decimal? CcgMaxLimitWithoutNeed { get; init; }

    public bool CcgSigned { get; init; }

    public IReadOnlyList<string> Reasons { get; init; } = [];
}

/// <summary>
/// Dados para enviar os termos preenchidos de uma proposta ao provedor (RN-063). A proposta é
/// identificada pelo id externo (o ProposalUniqueId devolvido na Cotação). Tags do objeto em
/// <see cref="Terms"/>; cada Cláusula particular marcada em <see cref="ParticularClauses"/>.
/// </summary>
public sealed record SubmitProposalTermsInput
{
    public required string BrokerCnpj { get; init; }

    public required string ProposalExternalId { get; init; }

    public IReadOnlyList<ProposalTermInput> Terms { get; init; } = [];

    public IReadOnlyList<ProposalParticularClauseInput> ParticularClauses { get; init; } = [];
}

/// <summary>Uma Tag preenchida (nome + valor) do objeto ou de uma cláusula (RN-063).</summary>
public sealed record ProposalTermInput(string Name, string Value);

/// <summary>Cláusula particular marcada + suas Tags preenchidas (RN-063).</summary>
public sealed record ProposalParticularClauseInput(int ParticularClauseId, IReadOnlyList<ProposalTermInput> Tags);

/// <summary>Minuta (documento) da proposta devolvida pelo provedor (RN-063): URL + id + data de geração.</summary>
public sealed record ProposalContractDraftResult(string? Url, string? ExternalId, DateTime? CreatedAt);

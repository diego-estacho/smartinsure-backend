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

    /// <summary>
    /// RN-504: submete a taxa nova à Seguradora (POST /UpdateProposalFinancialData), que devolve prêmio,
    /// comissão e opções de parcelamento recalculados. Chamada **mutante** de proposta → cliente sem nova
    /// tentativa automática (mesma razão da RN-057). Recusa da Seguradora e falha de transporte sobem como
    /// CalculationEngineException, e o caso de uso preserva os valores anteriores.
    /// </summary>
    Task<ProposalFinancialDataResult> UpdateProposalFinancialDataAsync(
        string? connectionParameters, UpdateProposalFinancialDataInput request, CancellationToken cancellationToken);

    /// <summary>
    /// RN-506: comunica à Seguradora que o corretor aceitou o Termo e declaração
    /// (POST /UpdatePolicyAcceptanceTerm), antes de solicitar a emissão. Mutação → sem retry (RN-057).
    /// </summary>
    Task SubmitPolicyAcceptanceTermAsync(
        string? connectionParameters, string brokerCnpj, string proposalExternalId, CancellationToken cancellationToken);

    /// <summary>
    /// RN-500/RN-514: solicita a emissão da Apólice (POST /CreatePolicy). Devolve a referência da apólice
    /// e o número da proposta — número da apólice, arquivo e boletos só na confirmação (fora desta fase).
    /// Mutação não repetível → cliente sem retry (RN-057). Recusa da Seguradora sobe como
    /// CalculationEngineException com a mensagem dela (RN-511).
    /// </summary>
    Task<PolicyIssuanceResult> CreatePolicyAsync(
        string? connectionParameters, CreatePolicyInput request, CancellationToken cancellationToken);

    /// <summary>
    /// RN-509: cancela a proposta de uma Cotação irmã na Seguradora (POST /CancelCotation), depois que
    /// outra Cotação do Grupo teve a emissão solicitada — proposta aberta tende a reter Limite de Crédito
    /// do Tomador. Mutação → sem retry (RN-057).
    /// </summary>
    Task CancelProposalAsync(
        string? connectionParameters, CancelProposalInput request, CancellationToken cancellationToken);
}

/// <summary>Dados do pedido de emissão enviado à Seguradora (RN-500/RN-503/RN-505).</summary>
public sealed record CreatePolicyInput
{
    public required string BrokerCnpj { get; init; }

    public required string ProposalExternalId { get; init; }

    /// <summary>Identificador da Seguradora no provedor.</summary>
    public required string InsuranceUniqueId { get; init; }

    /// <summary>RN-505: parcelamento escolhido entre os informados pela Seguradora.</summary>
    public required int InstallmentNumber { get; init; }

    /// <summary>RN-505: dias para o vencimento da primeira parcela.</summary>
    public required int GracePeriodInDays { get; init; }

    /// <summary>RN-503: endereço do Segurado da oferta — a Seguradora exige para emitir.</summary>
    public required IssuanceAddressInput InsuredAddress { get; init; }
}

/// <summary>Endereço enviado no pedido de emissão (RN-503).</summary>
public sealed record IssuanceAddressInput
{
    public string? ZipCode { get; init; }

    public string? Street { get; init; }

    public string? Number { get; init; }

    public string? Complement { get; init; }

    public string? Neighborhood { get; init; }

    public string? City { get; init; }

    public string? State { get; init; }
}

/// <summary>
/// Retorno do pedido de emissão (RN-514): a Seguradora devolve a referência da apólice e o número da
/// proposta. Número da apólice, arquivo e boletos vêm da confirmação — fora desta fase.
/// </summary>
public sealed record PolicyIssuanceResult
{
    public required string PolicyExternalId { get; init; }

    public string? ProposalNumber { get; init; }
}

/// <summary>Dados do cancelamento de proposta de uma Cotação irmã (RN-509).</summary>
public sealed record CancelProposalInput
{
    public required string BrokerCnpj { get; init; }

    public required string ProposalExternalId { get; init; }

    public required string Reason { get; init; }
}

/// <summary>Dados para submeter a taxa nova de uma proposta à Seguradora (RN-504).</summary>
public sealed record UpdateProposalFinancialDataInput
{
    public required string BrokerCnpj { get; init; }

    public required string ProposalExternalId { get; init; }

    /// <summary>Taxa pretendida; o limite aceitável é veredito da Seguradora, não da plataforma.</summary>
    public required decimal Tax { get; init; }
}

/// <summary>
/// Valores recalculados pela Seguradora após o ajuste da taxa (RN-504). Substituem os da Cotação
/// escolhida — a plataforma não recalcula dinheiro por conta própria (ADR-004).
/// </summary>
public sealed record ProposalFinancialDataResult
{
    public decimal? Premium { get; init; }

    public decimal? Tax { get; init; }

    public decimal? CommissionPercentage { get; init; }

    public decimal? CommissionValue { get; init; }

    public IReadOnlyList<QuotationInstallmentOption> InstallmentOptions { get; init; } = [];

    public IReadOnlyList<int> PossibleGracePeriodsInDays { get; init; } = [];
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

    /// <summary>
    /// RN-505: opções de parcelamento informadas pela Seguradora nesta Cotação. É delas que a etapa de
    /// emissão tira a forma de pagamento — a plataforma não calcula parcela nem oferece opção própria.
    /// </summary>
    public IReadOnlyList<QuotationInstallmentOption> InstallmentOptions { get; init; } = [];

    /// <summary>RN-505: dias possíveis para o vencimento da primeira parcela, informados pela Seguradora.</summary>
    public IReadOnlyList<int> PossibleGracePeriodsInDays { get; init; } = [];

    /// <summary>RN-510: documentos que a Seguradora exige para emitir; informativos ao corretor.</summary>
    public IReadOnlyList<QuotationRequiredDocument> RequiredDocuments { get; init; } = [];
}

/// <summary>Opção de parcelamento oferecida pela Seguradora numa Cotação (RN-505).</summary>
public sealed record QuotationInstallmentOption
{
    /// <summary>Número de parcelas.</summary>
    public required int Number { get; init; }

    /// <summary>Descrição da opção, como a Seguradora a apresenta.</summary>
    public string? Description { get; init; }

    /// <summary>Valor de cada parcela.</summary>
    public decimal Value { get; init; }

    /// <summary>Se a opção embute juros, conforme informado pela Seguradora.</summary>
    public bool HasInterest { get; init; }
}

/// <summary>Documento exigido pela Seguradora para emitir (RN-510).</summary>
public sealed record QuotationRequiredDocument
{
    public required string Name { get; init; }

    public string? Description { get; init; }
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

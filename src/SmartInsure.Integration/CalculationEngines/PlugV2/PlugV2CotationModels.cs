using System.Text.Json.Serialization;

namespace SmartInsure.Integration.CalculationEngines.PlugV2;

/// <summary>
/// Status imediato do resultado da Cotação, conforme o gateway PlugV2 define (ADR-064 — eixo imediato,
/// 11 valores conferidos na fonte). Modelo do fornecedor: não vaza da ACL (ADR-028). Qualquer valor
/// fora deste conjunto é tratado como Unrecognized pelo mapper.
/// </summary>
public enum EPlugApiStatus
{
    Unknow = 0,
    Success = 1,
    KanbanCadastro = 2,
    KanbanPep = 3,
    KanbanCredito = 4,
    KanbanSubscricao = 5,
    KanbanResseguro = 6,
    ModalidadeIndisponivel = 7,
    TomadorNomeado = 8,
    CoberturaIndisponivel = 9,
    Error = 99,
}

/// <summary>Request de Cotação do PlugV2 (POST /Cotation). Shape a confirmar no probe dev (T14).</summary>
public sealed record PlugV2CotationRequest
{
    [JsonPropertyName("BrokerCnpj")]
    public required string BrokerCnpj { get; init; }

    [JsonPropertyName("PolicyHolderCnpj")]
    public required string PolicyHolderCnpj { get; init; }

    [JsonPropertyName("InsuredCpfCnpj")]
    public required string InsuredCpfCnpj { get; init; }

    [JsonPropertyName("InsuranceUniqueId")]
    public required string InsuranceUniqueId { get; init; }

    [JsonPropertyName("ModalityGlobalId")]
    public required string ModalityGlobalId { get; init; }

    [JsonPropertyName("ModalityName")]
    public string? ModalityName { get; init; }

    [JsonPropertyName("InsuredAmountValue")]
    public required decimal InsuredAmountValue { get; init; }

    [JsonPropertyName("StartDate")]
    public required DateTime StartDate { get; init; }

    [JsonPropertyName("EndDate")]
    public required DateTime EndDate { get; init; }

    [JsonPropertyName("AdditionalCoverages")]
    public IReadOnlyList<string> AdditionalCoverages { get; init; } = [];
}

/// <summary>
/// Envelope padrão do gateway PlugV2 (BaseResponse&lt;T&gt;) — confirmado no probe dev (2026-07-28): o
/// /Cotation devolve o CotationResponse aninhado em <see cref="Response"/>, com StatusCode/HasError/
/// Errors no envelope. A ACL lê o payload (Response); HasError ou status HTTP de erro sinaliza falha
/// (RN-057). Corrige a leitura anterior, que lia os campos no topo (sempre nulos → Unrecognized).
/// </summary>
public sealed record PlugV2CotationResponse
{
    [JsonPropertyName("StatusCode")]
    public int StatusCode { get; init; }

    [JsonPropertyName("HasError")]
    public bool HasError { get; init; }

    [JsonPropertyName("Errors")]
    public List<string>? Errors { get; init; }

    [JsonPropertyName("Response")]
    public PlugV2CotationData? Response { get; init; }
}

/// <summary>
/// Payload da Cotação (CotationResponse) — o que vem dentro de <c>Response</c> do envelope. Só o eixo
/// imediato + prêmio/comissão/limite/CCG/motivos interessam à ACL (ADR-064). Nomes conferidos na fonte
/// legada (BusinessEntities.CotationResponse).
/// </summary>
public sealed record PlugV2CotationData
{
    [JsonPropertyName("ResponseStatus")]
    public PlugV2ResponseStatus? ResponseStatus { get; init; }

    [JsonPropertyName("Success")]
    public bool Success { get; init; }

    [JsonPropertyName("InsurancePremium")]
    public decimal? InsurancePremium { get; init; }

    [JsonPropertyName("ComissionPercentage")]
    public decimal? ComissionPercentage { get; init; }

    [JsonPropertyName("ComissionValue")]
    public decimal? ComissionValue { get; init; }

    [JsonPropertyName("Tax")]
    public decimal? Tax { get; init; }

    [JsonPropertyName("PolicyHolderAvailableLimit")]
    public decimal? PolicyHolderAvailableLimit { get; init; }

    [JsonPropertyName("ProposalUniqueId")]
    public string? ProposalUniqueId { get; init; }

    [JsonPropertyName("ProposalNumber")]
    public string? ProposalNumber { get; init; }

    [JsonPropertyName("Erros")]
    public List<string>? Erros { get; init; }

    [JsonPropertyName("PolicyHolderCCG")]
    public PlugV2CcgResult? PolicyHolderCcg { get; init; }

    /// <summary>RN-505: opções de parcelamento oferecidas pela Seguradora nesta Cotação.</summary>
    [JsonPropertyName("InstallmentOptions")]
    public IReadOnlyList<PlugV2InstallmentOption>? InstallmentOptions { get; init; }

    /// <summary>RN-505: dias possíveis para o vencimento da primeira parcela.</summary>
    [JsonPropertyName("PossibleGracePeriodsInDays")]
    public IReadOnlyList<int>? PossibleGracePeriodsInDays { get; init; }

    /// <summary>RN-510: documentos que a Seguradora exige para emitir.</summary>
    [JsonPropertyName("Documents")]
    public IReadOnlyList<PlugV2RequiredDocument>? Documents { get; init; }
}

/// <summary>Opção de parcelamento da Cotação (InstallmentOptionResponse) — RN-505.</summary>
public sealed record PlugV2InstallmentOption
{
    [JsonPropertyName("Number")]
    public int Number { get; init; }

    [JsonPropertyName("Description")]
    public string? Description { get; init; }

    [JsonPropertyName("Value")]
    public decimal Value { get; init; }

    [JsonPropertyName("HasInterest")]
    public bool HasInterest { get; init; }
}

/// <summary>Documento exigido pela Seguradora (RequestDocumentResponse) — RN-510.</summary>
public sealed record PlugV2RequiredDocument
{
    [JsonPropertyName("Name")]
    public string? Name { get; init; }

    [JsonPropertyName("Description")]
    public string? Description { get; init; }
}

/// <summary>Status imediato + mensagem do resultado (PlugResponseStatus).</summary>
public sealed record PlugV2ResponseStatus
{
    [JsonPropertyName("Status")]
    public int Status { get; init; }

    [JsonPropertyName("Message")]
    public string? Message { get; init; }
}

/// <summary>Veredito de CCG da Cotação (CotationPolicyHolderCCGResult) — ortogonal à classificação (ADR-064).</summary>
public sealed record PlugV2CcgResult
{
    [JsonPropertyName("RequiresCCG")]
    public bool RequiresCcg { get; init; }

    [JsonPropertyName("MaxLimitWithoutNeedCCG")]
    public decimal? MaxLimitWithoutNeedCcg { get; init; }

    [JsonPropertyName("HasSignedCCG")]
    public bool HasSignedCcg { get; init; }
}

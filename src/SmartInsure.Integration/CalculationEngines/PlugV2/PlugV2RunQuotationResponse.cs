using System.Text.Json.Serialization;

namespace SmartInsure.Integration.CalculationEngines.PlugV2;

/// <summary>
/// Envelope da resposta de POST /Cotation (RN-058). Shape a confirmar por probe dev (ADR-045):
/// o status imediato (11 valores) mais prêmio/condições, motivos e o veredito de CCG. O status
/// definitivo da proposta (recusa/cancelamento) não vem aqui (é followup, fora de escopo).
/// </summary>
public sealed record PlugV2RunQuotationResponse
{
    [JsonPropertyName("StatusCode")]
    public int StatusCode { get; init; }

    [JsonPropertyName("HasError")]
    public bool HasError { get; init; }

    [JsonPropertyName("Errors")]
    public List<string>? Errors { get; init; }

    [JsonPropertyName("Response")]
    public PlugV2QuotationData? Response { get; init; }
}

/// <summary>Dados da Cotação de uma Seguradora (por chamada). O eixo imediato é <see cref="Status"/>.</summary>
public sealed record PlugV2QuotationData
{
    /// <summary>Status imediato do resultado (nome estável — ex.: SUCCESS, KANBAN_SUBSCRICAO, ERROR).</summary>
    [JsonPropertyName("Status")]
    public string? Status { get; init; }

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

    [JsonPropertyName("Ccg")]
    public PlugV2QuotationCcg? Ccg { get; init; }
}

/// <summary>Veredito de Contragarantia da Cotação (RN-058, ADR-064). Ortogonal à classificação.</summary>
public sealed record PlugV2QuotationCcg
{
    [JsonPropertyName("RequiresCCG")]
    public bool RequiresCcg { get; init; }

    [JsonPropertyName("MaxLimitWithoutNeedCCG")]
    public decimal? MaxLimitWithoutNeed { get; init; }

    [JsonPropertyName("HasSignedCCG")]
    public bool HasSigned { get; init; }
}

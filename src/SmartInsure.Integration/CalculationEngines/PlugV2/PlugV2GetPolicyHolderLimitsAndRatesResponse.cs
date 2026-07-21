using System.Text.Json.Serialization;

namespace SmartInsure.Integration.CalculationEngines.PlugV2;

/// <summary>
/// Resposta de GetPolicyHolderLimitsAndRates do gateway PlugV2 (RN-029).
/// Campos são opcionais — Seguradora pode não retornar todas as modalidades.
/// </summary>
public sealed record PlugV2GetPolicyHolderLimitsAndRatesResponse
{
    [JsonPropertyName("success")]
    public required bool Success { get; init; }

    [JsonPropertyName("message")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Message { get; init; }

    [JsonPropertyName("traditionalLimit")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public decimal? TraditionalLimit { get; init; }

    [JsonPropertyName("traditionalRate")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public decimal? TraditionalRate { get; init; }

    [JsonPropertyName("judicialLimit")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public decimal? JudicialLimit { get; init; }

    [JsonPropertyName("judicialRate")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public decimal? JudicialRate { get; init; }

    [JsonPropertyName("judicialFiscalRate")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public decimal? JudicialFiscalRate { get; init; }

    [JsonPropertyName("financialLimit")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public decimal? FinancialLimit { get; init; }

    [JsonPropertyName("financialRate")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public decimal? FinancialRate { get; init; }

    [JsonPropertyName("limitValidUntil")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? LimitValidUntil { get; init; }
}

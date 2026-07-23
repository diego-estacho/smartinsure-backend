using System.Text.Json.Serialization;

namespace SmartInsure.Integration.CalculationEngines.PlugV2;

/// <summary>Requisição para GetPolicyHolderLimitsAndRates do gateway PlugV2 (RN-029).</summary>
public sealed record PlugV2GetPolicyHolderLimitsAndRatesRequest
{
    [JsonPropertyName("BrokerCnpj")]
    public required string BrokerCnpj { get; init; }

    [JsonPropertyName("PolicyHolderCnpj")]
    public required string PolicyHolderCnpj { get; init; }

    [JsonPropertyName("InsuranceUniqueId")]
    public required string InsuranceUniqueId { get; init; }
}

using System.Text.Json.Serialization;

namespace SmartInsure.Integration.CalculationEngines.PlugV2;

/// <summary>
/// Corpo de POST /Cotation do gateway PlugV2 (RN-056). Uma chamada cota UMA Seguradora
/// (InsuranceUniqueId). Shape a confirmar por probe dev (ADR-045) — os nomes seguem o contrato de
/// referência; a tradução do resultado fica na ACL (PlugV2QuotationAclMapper).
/// </summary>
public sealed record PlugV2RunQuotationRequest
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
    public required string ModalityName { get; init; }

    [JsonPropertyName("ModalityGroupType")]
    public string? ModalityGroupType { get; init; }

    [JsonPropertyName("InsuredAmountValue")]
    public required decimal InsuredAmountValue { get; init; }

    [JsonPropertyName("StartDate")]
    public required DateOnly StartDate { get; init; }

    [JsonPropertyName("EndDate")]
    public required DateOnly EndDate { get; init; }

    [JsonPropertyName("AdditionalCoverages")]
    public IReadOnlyList<string> AdditionalCoverages { get; init; } = [];
}

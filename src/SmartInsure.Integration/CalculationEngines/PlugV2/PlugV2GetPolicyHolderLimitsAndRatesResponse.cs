using System.Text.Json.Serialization;

namespace SmartInsure.Integration.CalculationEngines.PlugV2;

/// <summary>
/// Resposta envelope de GetPolicyHolderLimitsAndRates do gateway PlugV2 (RN-029).
/// Shape real validado em QA 2026-07-21: envelope StatusCode/HasError/Errors/Response[].
/// </summary>
public sealed record PlugV2GetPolicyHolderLimitsAndRatesResponse
{
    [JsonPropertyName("StatusCode")]
    public required int StatusCode { get; init; }

    [JsonPropertyName("HasError")]
    public required bool HasError { get; init; }

    [JsonPropertyName("Errors")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? Errors { get; init; }

    [JsonPropertyName("Response")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<PlugV2PolicyHolderResponse>? Response { get; init; }
}

/// <summary>
/// Item da resposta contendo informações da seguradora e limites agrupados por modalidade.
/// </summary>
public sealed record PlugV2PolicyHolderResponse
{
    [JsonPropertyName("Insurance")]
    public required PlugV2Insurance Insurance { get; init; }

    [JsonPropertyName("PolicyHolderName")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? PolicyHolderName { get; init; }

    [JsonPropertyName("PolicyHolderCnpj")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? PolicyHolderCnpj { get; init; }

    [JsonPropertyName("PolicyHolderUniqueId")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? PolicyHolderUniqueId { get; init; }

    [JsonPropertyName("CanSetupAProposal")]
    public required bool CanSetupAProposal { get; init; }

    [JsonPropertyName("LimitsAndRates")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<PlugV2LimitAndRate>? LimitsAndRates { get; init; }
}

/// <summary>
/// Informações da Seguradora dentro da resposta.
/// </summary>
public sealed record PlugV2Insurance
{
    [JsonPropertyName("Id")]
    public required int Id { get; init; }

    [JsonPropertyName("Name")]
    public required string Name { get; init; }

    [JsonPropertyName("InsuranceUniqueId")]
    public required string InsuranceUniqueId { get; init; }
}

/// <summary>
/// Limite e taxa por modalidade (agrupado pelo ModalityGroupName).
/// Valor do grupo = maior limite entre as modalidades que o compõem.
/// </summary>
public sealed record PlugV2LimitAndRate
{
    [JsonPropertyName("BranchName")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? BranchName { get; init; }

    [JsonPropertyName("BranchCode")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? BranchCode { get; init; }

    [JsonPropertyName("ModalityGroupName")]
    public required string ModalityGroupName { get; init; }

    [JsonPropertyName("ModalityGroupType")]
    public required string ModalityGroupType { get; init; }

    [JsonPropertyName("ModalityName")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ModalityName { get; init; }

    [JsonPropertyName("ModalityUniqueId")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ModalityUniqueId { get; init; }

    [JsonPropertyName("LimitRevised")]
    public required decimal LimitRevised { get; init; }

    [JsonPropertyName("AvailableLimit")]
    public required decimal AvailableLimit { get; init; }

    [JsonPropertyName("Tax")]
    public required decimal Tax { get; init; }
}

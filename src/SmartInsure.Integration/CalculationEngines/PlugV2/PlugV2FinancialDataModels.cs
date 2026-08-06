using System.Text.Json.Serialization;

namespace SmartInsure.Integration.CalculationEngines.PlugV2;

/// <summary>
/// Request do ajuste financeiro da proposta (POST /UpdateProposalFinancialData) — RN-504. Nomes
/// conferidos na fonte legada (UpdateProposalFinancialDataRequest).
/// </summary>
public sealed record PlugV2UpdateProposalFinancialDataRequest
{
    [JsonPropertyName("BrokerCnpj")]
    public required string BrokerCnpj { get; init; }

    [JsonPropertyName("ProposalUniqueId")]
    public required string ProposalUniqueId { get; init; }

    [JsonPropertyName("Tax")]
    public required decimal Tax { get; init; }
}

/// <summary>
/// Resposta do ajuste financeiro (UpdateProposalFinancialDataResponse) — RN-504. Só o eixo de dinheiro e
/// de pagamento interessa ao domínio; nada do modelo do fornecedor sai daqui (ADR-028).
/// </summary>
public sealed record PlugV2UpdateProposalFinancialDataResponse
{
    [JsonPropertyName("statusCode")]
    public int StatusCode { get; init; }

    [JsonPropertyName("hasError")]
    public bool HasError { get; init; }

    [JsonPropertyName("errors")]
    public List<string>? Errors { get; init; }

    [JsonPropertyName("response")]
    public PlugV2FinancialData? Response { get; init; }
}

/// <summary>
/// Request do aceite do Termo (POST /UpdatePolicyAcceptanceTerm) — RN-506. O gateway só recebe a
/// proposta: o texto aceito é prova nossa, registrada na plataforma.
/// </summary>
public sealed record PlugV2UpdatePolicyAcceptanceTermRequest
{
    [JsonPropertyName("BrokerCnpj")]
    public required string BrokerCnpj { get; init; }

    [JsonPropertyName("ProposalUniqueId")]
    public required string ProposalUniqueId { get; init; }
}

/// <summary>Request do pedido de emissão (POST /CreatePolicy) — RN-500/RN-505/RN-503.</summary>
public sealed record PlugV2CreatePolicyRequest
{
    [JsonPropertyName("BrokerCnpj")]
    public required string BrokerCnpj { get; init; }

    [JsonPropertyName("ProposalUniqueId")]
    public required string ProposalUniqueId { get; init; }

    [JsonPropertyName("InsuranceUniqueId")]
    public required string InsuranceUniqueId { get; init; }

    [JsonPropertyName("InstallmentNumber")]
    public required int InstallmentNumber { get; init; }

    /// <summary>Dias para o vencimento da primeira parcela (GracePeriod no contrato do gateway).</summary>
    [JsonPropertyName("GracePeriod")]
    public required int GracePeriod { get; init; }

    [JsonPropertyName("InsuredLocation")]
    public required PlugV2PersonLocation InsuredLocation { get; init; }
}

/// <summary>Endereço no contrato do gateway (PersonLocationModel) — RN-503.</summary>
public sealed record PlugV2PersonLocation
{
    [JsonPropertyName("ZipCode")]
    public string? ZipCode { get; init; }

    [JsonPropertyName("AddressName")]
    public string? AddressName { get; init; }

    [JsonPropertyName("Number")]
    public string? Number { get; init; }

    [JsonPropertyName("Complement")]
    public string? Complement { get; init; }

    [JsonPropertyName("Neighborhood")]
    public string? Neighborhood { get; init; }

    [JsonPropertyName("CityName")]
    public string? CityName { get; init; }

    [JsonPropertyName("StateProvinceName")]
    public string? StateProvinceName { get; init; }
}

/// <summary>
/// Resposta do pedido de emissão (PolicyResponse) — RN-514. O CreatePolicy do gateway preenche apenas a
/// referência da apólice, o número e o id da proposta; número da apólice, arquivo e boletos só na
/// consulta posterior (fora desta fase).
/// </summary>
public sealed record PlugV2CreatePolicyResponse
{
    [JsonPropertyName("statusCode")]
    public int StatusCode { get; init; }

    [JsonPropertyName("hasError")]
    public bool HasError { get; init; }

    [JsonPropertyName("errors")]
    public List<string>? Errors { get; init; }

    [JsonPropertyName("response")]
    public PlugV2PolicyData? Response { get; init; }
}

/// <summary>Payload da apólice devolvido pelo gateway (RN-514).</summary>
public sealed record PlugV2PolicyData
{
    [JsonPropertyName("PolicyUniqueId")]
    public string? PolicyUniqueId { get; init; }

    [JsonPropertyName("ProposalNumber")]
    public string? ProposalNumber { get; init; }

    [JsonPropertyName("PolicyNumber")]
    public string? PolicyNumber { get; init; }
}

/// <summary>Request do cancelamento de proposta (POST /CancelCotation) — RN-509.</summary>
public sealed record PlugV2CancelProposalRequest
{
    [JsonPropertyName("BrokerCnpj")]
    public required string BrokerCnpj { get; init; }

    [JsonPropertyName("ProposalUniqueId")]
    public required string ProposalUniqueId { get; init; }

    [JsonPropertyName("Reason")]
    public string? Reason { get; init; }
}

/// <summary>Payload do ajuste financeiro: valores recalculados pela Seguradora (RN-504).</summary>
public sealed record PlugV2FinancialData
{
    [JsonPropertyName("Success")]
    public bool Success { get; init; }

    [JsonPropertyName("Erros")]
    public List<string>? Erros { get; init; }

    [JsonPropertyName("InsurancePremium")]
    public decimal? InsurancePremium { get; init; }

    [JsonPropertyName("Tax")]
    public decimal? Tax { get; init; }

    [JsonPropertyName("ComissionPercentage")]
    public decimal? ComissionPercentage { get; init; }

    [JsonPropertyName("ComissionValue")]
    public decimal? ComissionValue { get; init; }

    [JsonPropertyName("InstallmentOptions")]
    public IReadOnlyList<PlugV2InstallmentOption>? InstallmentOptions { get; init; }

    [JsonPropertyName("PossibleGracePeriodsInDays")]
    public IReadOnlyList<int>? PossibleGracePeriodsInDays { get; init; }
}

using System.Text.Json.Serialization;

namespace SmartInsure.Integration.CalculationEngines.PlugV2;

/// <summary>
/// Request de envio dos termos preenchidos da proposta (POST /UpdateProposalTerms — RN-063). Espelha o
/// contrato legado (UpdateProposalTermsRequest): as Tags do objeto em <see cref="Terms"/> e cada Cláusula
/// particular marcada em <see cref="ParticularClauses"/> com suas próprias Tags. Shape do fornecedor —
/// não vaza da ACL (ADR-028).
/// </summary>
public sealed record PlugV2UpdateProposalTermsRequest
{
    [JsonPropertyName("BrokerCnpj")]
    public required string BrokerCnpj { get; init; }

    [JsonPropertyName("ProposalUniqueId")]
    public required string ProposalUniqueId { get; init; }

    [JsonPropertyName("Terms")]
    public IReadOnlyList<PlugV2ProposalTerm> Terms { get; init; } = [];

    [JsonPropertyName("ParticularClauses")]
    public IReadOnlyList<PlugV2ProposalParticularClause> ParticularClauses { get; init; } = [];
}

/// <summary>Uma Tag preenchida do objeto/cláusula (nome + valor). Legado: Terms { Name, Values }.</summary>
public sealed record PlugV2ProposalTerm
{
    [JsonPropertyName("Name")]
    public required string Name { get; init; }

    [JsonPropertyName("Values")]
    public required string Values { get; init; }
}

/// <summary>Cláusula particular marcada + suas Tags. Legado: ParticularClauseTerms { ParticularClauseId, Tags }.</summary>
public sealed record PlugV2ProposalParticularClause
{
    [JsonPropertyName("ParticularClauseId")]
    public int ParticularClauseId { get; init; }

    [JsonPropertyName("Tags")]
    public IReadOnlyList<PlugV2ProposalTerm> Tags { get; init; } = [];
}

/// <summary>Request da minuta (documento) da proposta (POST /GetProposalContractDraft — RN-063).</summary>
public sealed record PlugV2GetProposalContractDraftRequest
{
    [JsonPropertyName("BrokerCnpj")]
    public required string BrokerCnpj { get; init; }

    [JsonPropertyName("ProposalUniqueId")]
    public required string ProposalUniqueId { get; init; }
}

/// <summary>Envelope do gateway para a minuta (BaseResponse&lt;ProposalContractOnPointResponse&gt;).</summary>
public sealed record PlugV2ProposalContractDraftResponse
{
    [JsonPropertyName("StatusCode")]
    public int StatusCode { get; init; }

    [JsonPropertyName("HasError")]
    public bool HasError { get; init; }

    [JsonPropertyName("Errors")]
    public List<string>? Errors { get; init; }

    [JsonPropertyName("Response")]
    public PlugV2ProposalContractDraftData? Response { get; init; }
}

/// <summary>Payload da minuta — o documento gerado (UniqueId + data + URL). Legado: ProposalContractOnPointResponse.</summary>
public sealed record PlugV2ProposalContractDraftData
{
    [JsonPropertyName("UniqueId")]
    public string? UniqueId { get; init; }

    [JsonPropertyName("CreateDate")]
    public DateTime? CreateDate { get; init; }

    [JsonPropertyName("Url")]
    public string? Url { get; init; }
}

/// <summary>Envelope mínimo do gateway para leitura de erros em respostas de falha (StatusCode/HasError/Errors).</summary>
public sealed record PlugV2ErrorEnvelope
{
    [JsonPropertyName("StatusCode")]
    public int StatusCode { get; init; }

    [JsonPropertyName("HasError")]
    public bool HasError { get; init; }

    [JsonPropertyName("Errors")]
    public List<string>? Errors { get; init; }
}

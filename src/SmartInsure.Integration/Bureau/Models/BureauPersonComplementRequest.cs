using System.Text.Json.Serialization;

namespace SmartInsure.Integration.Bureau.Models;

/// <summary>Corpo do POST GetPersonComplement no gateway de Bureau.</summary>
public sealed record BureauPersonComplementRequest
{
    [JsonPropertyName("cpfCnpj")]
    public required string CpfCnpj { get; init; }

    [JsonPropertyName("tipoPessoa")]
    public required string TipoPessoa { get; init; }

    [JsonPropertyName("bureauChoices")]
    public required IReadOnlyList<string> BureauChoices { get; init; }
}

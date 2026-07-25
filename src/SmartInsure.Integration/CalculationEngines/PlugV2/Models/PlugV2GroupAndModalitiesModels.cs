using System.Text.Json;

namespace SmartInsure.Integration.CalculationEngines.PlugV2.Models;

/// <summary>Envelope padrão do PlugV2 (modelo do fornecedor — nunca vaza da ACL, ADR-045).</summary>
internal sealed class PlugV2BaseResponse<T>
{
    public int StatusCode { get; set; }

    public T? Response { get; set; }

    public bool HasError { get; set; }

    public List<string>? Errors { get; set; }
}

internal sealed class PlugV2GroupsAndModalities
{
    public PlugV2Insurance? Insurance { get; set; }

    public List<PlugV2GlobalModality>? GlobalModalities { get; set; }

    public bool IsSuccess { get; set; }
}

internal sealed class PlugV2Insurance
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? InsuranceUniqueId { get; set; }
}

internal sealed class PlugV2GlobalModality
{
    public int Id { get; set; }

    public string? Name { get; set; }

    /// <summary>Cada modalidade é preservada como veio (RN-033) — mantida como JSON cru.</summary>
    public List<JsonElement>? Modalities { get; set; }
}

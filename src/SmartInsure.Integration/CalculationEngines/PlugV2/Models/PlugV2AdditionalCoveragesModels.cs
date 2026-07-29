namespace SmartInsure.Integration.CalculationEngines.PlugV2.Models;

/// <summary>Payload de GetAdditionalCoverages (modelo do fornecedor — nunca vaza da ACL, ADR-045).</summary>
internal sealed class PlugV2AdditionalCoveragesResponse
{
    public List<PlugV2AdditionalCoverageEntry>? AdditionalCoverages { get; set; }
}

internal sealed class PlugV2AdditionalCoverageEntry
{
    public string? BranchName { get; set; }

    public string? BranchCode { get; set; }

    public PlugV2AdditionalCoverageItem? AdditionalCoverages { get; set; }
}

internal sealed class PlugV2AdditionalCoverageItem
{
    public string? Name { get; set; }

    public string? UniqueId { get; set; }

    public int InsuredAmountCalculationType { get; set; }

    public bool AllowManualEdit { get; set; }
}

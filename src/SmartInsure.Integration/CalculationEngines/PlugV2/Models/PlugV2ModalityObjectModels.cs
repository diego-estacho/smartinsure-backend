namespace SmartInsure.Integration.CalculationEngines.PlugV2.Models;

/// <summary>Payload de resposta de GetModalityObject (modelo do fornecedor — nunca vaza da ACL, ADR-045).</summary>
internal sealed class PlugV2ModalityObjectResponse
{
    public PlugV2ModalityObjectData? Object { get; set; }

    public List<PlugV2ParticularClause>? ParticularClauses { get; set; }
}

internal sealed class PlugV2ModalityObjectData
{
    public string? Text { get; set; }

    public string? JsonTag { get; set; }
}

internal sealed class PlugV2ParticularClause
{
    /// <summary>Numérico na origem; ausente (null) sinaliza cláusula sem id — descartada (RN-041).</summary>
    public int? Id { get; set; }

    public string? Name { get; set; }

    public string? Text { get; set; }

    public string? JsonTag { get; set; }
}

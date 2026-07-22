namespace SmartInsure.Core.Entities;

/// <summary>
/// Grupo Importado (RN-030): o agrupador tal como a Seguradora o manda, preservado como veio,
/// para conferência e rastreio. Reencontrado pelo identificador de origem, por Seguradora.
/// </summary>
public sealed class ImportedGroup : EntityBase
{
    private ImportedGroup()
    {
    }

    public Guid InsurerId { get; private set; }

    public string SourceId { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string? Type { get; private set; }

    public static ImportedGroup Create(Guid insurerId, string sourceId, string name, string? type)
    {
        var group = new ImportedGroup { InsurerId = insurerId, SourceId = sourceId.Trim() };
        group.SetDetails(name, type);
        return group;
    }

    /// <summary>RN-030: nome e tipo refletem sempre a última importação da Seguradora.</summary>
    public void UpdateFromSource(string name, string? type) => SetDetails(name, type);

    private void SetDetails(string name, string? type)
    {
        Name = name.Trim();
        Type = string.IsNullOrWhiteSpace(type) ? null : type.Trim();
    }
}

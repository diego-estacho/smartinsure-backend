using SmartInsure.Core.Enumerators;

namespace SmartInsure.Core.Entities;

/// <summary>
/// Cláusula particular (RN-041): texto contratual opcional da Modalidade Importada, entregue no mesmo
/// objeto da modalidade. Identidade por (Modalidade Importada, id externo da cláusula na OnPoint).
/// Nasce Ativa; reaparecer reativa; ausência numa consulta bem-sucedida inativa (RN-042).
/// </summary>
public sealed class ImportedModalityParticularClause : EntityBase
{
    private ImportedModalityParticularClause()
    {
    }

    public Guid ImportedModalityId { get; private set; }

    public string ExternalId { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string? ClauseText { get; private set; }

    public string? JsonTag { get; private set; }

    public EImportedModalityClauseStatus Status { get; private set; }

    public static ImportedModalityParticularClause Create(
        Guid importedModalityId, string externalId, string name, string? text, string? jsonTag)
    {
        var clause = new ImportedModalityParticularClause
        {
            ImportedModalityId = importedModalityId,
            ExternalId = externalId.Trim(),
        };
        clause.UpdateFromSource(name, text, jsonTag);
        return clause;
    }

    public void UpdateFromSource(string name, string? text, string? jsonTag)
    {
        Name = name.Trim();
        ClauseText = string.IsNullOrWhiteSpace(text) ? null : text;
        JsonTag = string.IsNullOrWhiteSpace(jsonTag) ? null : jsonTag;
        Status = EImportedModalityClauseStatus.Active;
    }

    public void Deactivate() => Status = EImportedModalityClauseStatus.Inactive;
}

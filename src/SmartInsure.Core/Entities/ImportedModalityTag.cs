using SmartInsure.Core.Enumerators;

namespace SmartInsure.Core.Entities;

/// <summary>
/// Tag da Modalidade Importada (RN-047): o desenho do formulário (JsonTag) do objeto da modalidade,
/// mantido pela OnPoint e trazido embutido no objeto. 1:1 com a Modalidade Importada. Só gravada com
/// JsonTag preenchido; nunca sobrescreve com vazio. Nasce Ativa; reaparecer reativa.
/// </summary>
public sealed class ImportedModalityTag : EntityBase
{
    private ImportedModalityTag()
    {
    }

    public Guid ImportedModalityId { get; private set; }

    public string JsonTag { get; private set; } = string.Empty;

    public string? ObjectText { get; private set; }

    public EImportedModalityTagStatus Status { get; private set; }

    public static ImportedModalityTag Create(Guid importedModalityId, string jsonTag, string? objectText)
    {
        var tag = new ImportedModalityTag { ImportedModalityId = importedModalityId };
        tag.UpdateFromSource(jsonTag, objectText);
        return tag;
    }

    /// <summary>RN-047: atualiza o conteúdo da fonte e reativa. Chamado só quando há JsonTag preenchido.</summary>
    public void UpdateFromSource(string jsonTag, string? objectText)
    {
        JsonTag = jsonTag.Trim();
        ObjectText = string.IsNullOrWhiteSpace(objectText) ? null : objectText;
        Status = EImportedModalityTagStatus.Active;
    }

    /// <summary>RN-049: reconciliação — ausência numa consulta bem-sucedida inativa (nunca apaga). Idempotente.</summary>
    public void Deactivate() => Status = EImportedModalityTagStatus.Inactive;
}

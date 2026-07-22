using SmartInsure.Core.Enumerators;

namespace SmartInsure.Core.Entities;

/// <summary>
/// Modalidade Importada (RN-030): a modalidade como a Seguradora a oferece, trazida na importação
/// exatamente como exposta. Reencontrada pelo identificador de origem (SourceId), por Seguradora.
/// Nome, ramo, público-alvo e parâmetros comerciais vêm da fonte e não são editados à mão (RN-030).
/// </summary>
public sealed class ImportedModality : EntityBase
{
    private ImportedModality()
    {
    }

    public Guid InsurerId { get; private set; }

    /// <summary>Identificador de origem (ModalityUniqueId no PlugV2) — chave de reencontro.</summary>
    public string SourceId { get; private set; } = string.Empty;

    public string OriginName { get; private set; } = string.Empty;

    public ESuretyBranch Branch { get; private set; }

    /// <summary>Identificador da Modalidade Global (GlobalModalities[].Id no PlugV2) — resolve o vínculo (RN-032).</summary>
    public string? EngineModalityId { get; private set; }

    public string? EngineModalityName { get; private set; }

    public Guid? ImportedGroupId { get; private set; }

    /// <summary>RN-032/RN-034: Modalidade (lado Smart) a que esta Importada está vinculada. Nulo = exceção sem vínculo (vai à Fila).</summary>
    public Guid? ModalityId { get; private set; }

    /// <summary>RN-032/RN-034: origem do vínculo — Automatic (id global) ou Manual (override do Administrador).</summary>
    public EModalityLinkSource? ModalityLinkSource { get; private set; }

    /// <summary>Parâmetros comerciais preservados como recebidos da fonte (RN-030).</summary>
    public string? CommercialParameters { get; private set; }

    public EImportedModalityStatus Status { get; private set; }

    /// <summary>RN-034: marcada como Ignorada na Fila — não é oferecida e não volta à fila. A importação não altera este marcador.</summary>
    public bool IsIgnored { get; private set; }

    public DateTime LastImportedAt { get; private set; }

    public static ImportedModality Create(
        Guid insurerId,
        string sourceId,
        string originName,
        ESuretyBranch branch,
        string? engineModalityId,
        string? engineModalityName,
        Guid? importedGroupId,
        string? commercialParameters,
        DateTime lastImportedAt)
    {
        var modality = new ImportedModality
        {
            InsurerId = insurerId,
            SourceId = sourceId.Trim(),
            Status = EImportedModalityStatus.Active,
        };
        modality.SetFromSource(
            originName, branch, engineModalityId, engineModalityName, importedGroupId,
            commercialParameters, lastImportedAt);
        return modality;
    }

    /// <summary>
    /// RN-030: atualiza com os dados atuais da fonte, preservando identidade e mapeamento.
    /// RN-035: presença numa importação bem-sucedida reativa a Modalidade Importada.
    /// </summary>
    public void UpdateFromSource(
        string originName,
        ESuretyBranch branch,
        string? engineModalityId,
        string? engineModalityName,
        Guid? importedGroupId,
        string? commercialParameters,
        DateTime lastImportedAt)
    {
        SetFromSource(
            originName, branch, engineModalityId, engineModalityName, importedGroupId,
            commercialParameters, lastImportedAt);
        Status = EImportedModalityStatus.Active;
    }

    /// <summary>
    /// RN-032/RN-034: vincula à Modalidade. A importação (Automatic) resolve pelo id da Modalidade
    /// Global; um override Manual do Administrador é preservado — a automação nunca o sobrescreve.
    /// </summary>
    public void LinkToModality(Guid modalityId, EModalityLinkSource source)
    {
        if (ModalityId is not null
            && ModalityLinkSource == EModalityLinkSource.Manual
            && source == EModalityLinkSource.Automatic)
        {
            return;
        }

        ModalityId = modalityId;
        ModalityLinkSource = source;
    }

    /// <summary>RN-035: ausência numa importação bem-sucedida da Seguradora desativa. Idempotente (automação).</summary>
    public void Deactivate() => Status = EImportedModalityStatus.Inactive;

    /// <summary>RN-034: o Administrador marca a Modalidade Importada como Ignorada na Fila.</summary>
    public void Ignore() => IsIgnored = true;

    /// <summary>RN-034: reavaliar um item antes ignorado — volta a poder entrar na Fila.</summary>
    public void Restore() => IsIgnored = false;

    private void SetFromSource(
        string originName,
        ESuretyBranch branch,
        string? engineModalityId,
        string? engineModalityName,
        Guid? importedGroupId,
        string? commercialParameters,
        DateTime lastImportedAt)
    {
        OriginName = originName.Trim();
        Branch = branch;
        EngineModalityId = string.IsNullOrWhiteSpace(engineModalityId) ? null : engineModalityId.Trim();
        EngineModalityName = string.IsNullOrWhiteSpace(engineModalityName) ? null : engineModalityName.Trim();
        ImportedGroupId = importedGroupId;
        CommercialParameters = string.IsNullOrWhiteSpace(commercialParameters) ? null : commercialParameters;
        LastImportedAt = lastImportedAt;
    }
}

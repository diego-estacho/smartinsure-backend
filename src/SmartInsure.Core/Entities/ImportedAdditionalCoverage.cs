using SmartInsure.Core.Enumerators;

namespace SmartInsure.Core.Entities;

/// <summary>
/// Cobertura Adicional Importada (RN-041): a garantia complementar como uma Seguradora a expõe na
/// OnPoint, por Modalidade Importada, trazida na importação exatamente como veio. Reencontrada por
/// (Modalidade Importada, nome). Nome, identificador de origem, tipo de cálculo e edição manual vêm
/// da fonte e não são editados à mão; tipo de cálculo e edição manual são preservados como recebidos
/// (OPEN-16). Vinculada manualmente a uma Cobertura Adicional canônica pelo Administrador (RN-043);
/// sem vínculo e não Ignorada, fica pendente de mapeamento. Nunca excluída; sai por Inativação (RN-044).
/// </summary>
public sealed class ImportedAdditionalCoverage : EntityBase
{
    private ImportedAdditionalCoverage()
    {
    }

    public Guid ImportedModalityId { get; private set; }

    /// <summary>Nome de origem normalizado (trim) — parte da identidade (RN-041).</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Identificador de origem da OnPoint (UniqueId); preservado como recebido, pode ser ausente.</summary>
    public string? SourceUniqueId { get; private set; }

    /// <summary>Tipo de cálculo do valor segurado da OnPoint — armazenado cru, não interpretado (OPEN-16).</summary>
    public int InsuredAmountCalculationType { get; private set; }

    /// <summary>Indicação da OnPoint de edição manual permitida — armazenada crua, não interpretada (OPEN-16).</summary>
    public bool AllowManualEdit { get; private set; }

    /// <summary>RN-043: Cobertura Adicional canônica vinculada manualmente pelo Administrador. Nulo = pendente de mapeamento.</summary>
    public Guid? AdditionalCoverageId { get; private set; }

    /// <summary>RN-043: marcada como Ignorada — não aparece como pendente nem é oferecida; não volta à pendência. A importação não altera este marcador.</summary>
    public bool IsIgnored { get; private set; }

    public EImportedAdditionalCoverageStatus Status { get; private set; }

    public DateTime LastImportedAt { get; private set; }

    public static ImportedAdditionalCoverage Create(
        Guid importedModalityId,
        string name,
        string? sourceUniqueId,
        int insuredAmountCalculationType,
        bool allowManualEdit,
        DateTime lastImportedAt)
    {
        var coverage = new ImportedAdditionalCoverage
        {
            ImportedModalityId = importedModalityId,
            Name = name.Trim(),
            Status = EImportedAdditionalCoverageStatus.Active,
        };
        coverage.SetFromSource(sourceUniqueId, insuredAmountCalculationType, allowManualEdit, lastImportedAt);
        return coverage;
    }

    /// <summary>
    /// RN-041/RN-044: atualiza com os dados atuais da fonte e reativa — presença numa consulta
    /// bem-sucedida reativa. A identidade (Modalidade Importada + nome) e o vínculo manual (RN-043) não mudam.
    /// </summary>
    public void UpdateFromSource(
        string? sourceUniqueId, int insuredAmountCalculationType, bool allowManualEdit, DateTime lastImportedAt)
    {
        SetFromSource(sourceUniqueId, insuredAmountCalculationType, allowManualEdit, lastImportedAt);
        Status = EImportedAdditionalCoverageStatus.Active;
    }

    /// <summary>RN-044: ausência numa consulta bem-sucedida da Modalidade Importada desativa. Idempotente (automação).</summary>
    public void Deactivate() => Status = EImportedAdditionalCoverageStatus.Inactive;

    /// <summary>RN-043: o Administrador vincula (ou reatribui) a importada a uma Cobertura Adicional canônica.</summary>
    public void LinkTo(Guid additionalCoverageId) => AdditionalCoverageId = additionalCoverageId;

    /// <summary>RN-043: o Administrador desfaz o vínculo — a importada volta a pendente de mapeamento.</summary>
    public void Unlink() => AdditionalCoverageId = null;

    /// <summary>RN-043: o Administrador ignora uma importada que não deve ser mapeada — sai das pendências.</summary>
    public void Ignore() => IsIgnored = true;

    /// <summary>RN-043: reavaliar uma importada antes Ignorada — volta a poder ser pendente/mapeada.</summary>
    public void Restore() => IsIgnored = false;

    private void SetFromSource(
        string? sourceUniqueId, int insuredAmountCalculationType, bool allowManualEdit, DateTime lastImportedAt)
    {
        SourceUniqueId = string.IsNullOrWhiteSpace(sourceUniqueId) ? null : sourceUniqueId.Trim();
        InsuredAmountCalculationType = insuredAmountCalculationType;
        AllowManualEdit = allowManualEdit;
        LastImportedAt = lastImportedAt;
    }
}

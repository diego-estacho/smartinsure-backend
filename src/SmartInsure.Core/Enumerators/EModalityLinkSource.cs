namespace SmartInsure.Core.Enumerators;

/// <summary>
/// Origem do vínculo Modalidade Importada → Modalidade (glossário, revisto 2026-07-22). Persistido
/// como string (ADR-031). <see cref="Automatic"/>: resolvido na importação pelo id da Modalidade
/// Global (RN-035). <see cref="Manual"/>: override do Administrador na Fila — preservado na
/// reimportação, a importação automática nunca o sobrescreve (RN-037).
/// </summary>
public enum EModalityLinkSource
{
    Automatic,
    Manual,
}

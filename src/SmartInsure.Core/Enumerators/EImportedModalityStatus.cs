namespace SmartInsure.Core.Enumerators;

/// <summary>
/// Situação da Modalidade Importada (glossário, proposto 2026-07-21). Persistido como string (ADR-031).
/// Passa a Inativa automaticamente quando some de uma importação bem-sucedida da Seguradora (RN-035);
/// reaparecendo, é reativada.
/// </summary>
public enum EImportedModalityStatus
{
    Active,
    Inactive,
}

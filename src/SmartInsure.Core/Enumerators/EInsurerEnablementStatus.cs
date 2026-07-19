namespace SmartInsure.Core.Enumerators;

/// <summary>
/// Situação da Habilitação de Seguradora (glossário, proposto 2026-07-19). Persistido
/// como string (ADR-031). Transições: Ativa (Active) ↔ Inativa (Inactive) — RN-022.
/// </summary>
public enum EInsurerEnablementStatus
{
    Active,
    Inactive,
}

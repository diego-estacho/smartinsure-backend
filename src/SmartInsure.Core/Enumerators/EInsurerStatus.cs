namespace SmartInsure.Core.Enumerators;

/// <summary>
/// Situação da Seguradora (glossário, proposto 2026-07-16). Persistido como string (ADR-031).
/// Transições: Ativa (Active) ↔ Inativa (Inactive) — RN-009.
/// </summary>
public enum EInsurerStatus
{
    Active,
    Inactive,
}

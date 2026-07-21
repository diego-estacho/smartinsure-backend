namespace SmartInsure.Core.Enumerators;

/// <summary>
/// Situação da Modalidade (glossário, proposto 2026-07-21). Persistido como string (ADR-031).
/// Transições: Ativa (Active) ↔ Inativa (Inactive) — RN-036. Inativa retira a Modalidade da operação sem apagá-la.
/// </summary>
public enum EModalityStatus
{
    Active,
    Inactive,
}

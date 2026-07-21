namespace SmartInsure.Core.Enumerators;

/// <summary>
/// Situação do Grupo de Modalidade (glossário, proposto 2026-07-21). Persistido como string (ADR-031).
/// Transições: Ativa (Active) ↔ Inativa (Inactive) — RN-036. Inativa esconde o grupo e suas Modalidades da operação.
/// </summary>
public enum EModalityGroupStatus
{
    Active,
    Inactive,
}

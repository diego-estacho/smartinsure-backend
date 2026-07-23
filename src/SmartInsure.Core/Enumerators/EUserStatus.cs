namespace SmartInsure.Core.Enumerators;

/// <summary>
/// Situação do Usuário (glossário). Persistido como string (ADR-031).
/// Transições: Pendente (Pending) → Ativo (Active) — RN-002; Ativo ↔ Inativo (Inactive) — RN-046.
/// </summary>
public enum EUserStatus
{
    Pending,
    Active,
    Inactive,
}

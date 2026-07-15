namespace SmartInsure.Core.Enumerators;

/// <summary>
/// Situação do Usuário (glossário, ratificado 2026-07-15). Persistido como string (ADR-031).
/// Transição permitida: Pendente (Pending) → Ativo (Active) — RN-002.
/// </summary>
public enum EUserStatus
{
    Pending,
    Active,
}

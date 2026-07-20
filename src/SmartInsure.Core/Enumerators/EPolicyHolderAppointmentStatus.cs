namespace SmartInsure.Core.Enumerators;

/// <summary>
/// Status da Nomeação de Tomador (glossário, RN-027/RN-028). Persistido como string.
/// Transições: Active → Ended (unidirecional; Ended nunca volta a Active).
/// </summary>
public enum EPolicyHolderAppointmentStatus
{
    Active,
    Ended,
}

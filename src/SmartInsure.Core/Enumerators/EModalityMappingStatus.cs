namespace SmartInsure.Core.Enumerators;

/// <summary>
/// Situação do Mapeamento de Modalidade (glossário, proposto 2026-07-21). Persistido como string (ADR-031).
/// Confirmado vale para a operação; Pendente aguarda revisão na Fila (RN-033/RN-034).
/// </summary>
public enum EModalityMappingStatus
{
    Confirmed,
    Pending,
}

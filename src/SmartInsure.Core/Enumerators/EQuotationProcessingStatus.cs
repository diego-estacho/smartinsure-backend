namespace SmartInsure.Core.Enumerators;

/// <summary>
/// Estado de processamento de uma Cotação no fan-out assíncrono (RN-057, ADR-050): nasce
/// <see cref="Requested"/> (persistida antes de enfileirar), e o consumidor a leva a
/// <see cref="Obtained"/> (Seguradora respondeu) ou <see cref="Failed"/> (falha/timeout isolado).
/// O reconciliador reenfileira as que ficaram <see cref="Requested"/>. Persistido como string (ADR-031).
/// </summary>
public enum EQuotationProcessingStatus
{
    Requested,
    Obtained,
    Failed,
}

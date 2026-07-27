namespace SmartInsure.Core.Enumerators;

/// <summary>
/// Situação do Grupo de Cotação (glossário, proposto 2026-07-24 — RN-050/RN-051). Persistido como
/// string (ADR-031). Nesta fase há um único estado — Rascunho (Draft); os estados posteriores
/// (cotação obtida, proposta aceita, apólice emitida) dependem de ratificação da PO (OPEN-07).
/// </summary>
public enum EQuotationGroupStatus
{
    Draft,
}

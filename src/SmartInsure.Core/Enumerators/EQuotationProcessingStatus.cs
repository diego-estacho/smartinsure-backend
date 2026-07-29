namespace SmartInsure.Core.Enumerators;

/// <summary>
/// Estado de processamento do fan-out por Seguradora (RN-057), distinto do resultado de negócio
/// (EQuotationResult): Requested (solicitada, aguardando resposta), Obtained (respondida pela
/// Seguradora) e Failed (falha/timeout ao obter). Persistido como string (ADR-031).
/// </summary>
public enum EQuotationProcessingStatus
{
    Requested,
    Obtained,
    Failed,
}

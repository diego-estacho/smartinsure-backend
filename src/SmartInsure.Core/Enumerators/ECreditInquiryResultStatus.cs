namespace SmartInsure.Core.Enumerators;

/// <summary>
/// Status do resultado da Consulta de Crédito por Seguradora (RN-029/RN-030).
/// Persistido como string (ADR-031). Disponível (Available) ou Indisponível (Unavailable) com motivo.
/// </summary>
public enum ECreditInquiryResultStatus
{
    Available,
    Unavailable,
}

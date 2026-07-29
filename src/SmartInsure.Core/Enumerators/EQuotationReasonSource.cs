namespace SmartInsure.Core.Enumerators;

/// <summary>
/// Origem de um motivo de indisponibilidade/recusa de uma Cotação (RN-056/RN-058): Provider (informado
/// pela Seguradora) ou Local (gerado pela plataforma — ex.: Seguradora habilitada não incluída na
/// solicitação no modo Specific). Persistido como string (ADR-031).
/// </summary>
public enum EQuotationReasonSource
{
    Provider,
    Local,
}

namespace SmartInsure.Core.Enumerators;

/// <summary>
/// Resultado (classificação estável) de uma Cotação, traduzido da resposta da Seguradora num único
/// ponto — a ACL do provedor (RN-058, ADR-064). Conjunto pequeno e fechado; motivo, esteira, prêmio
/// e CCG são dados que acompanham, não classificações novas. Todo status não reconhecido recai em
/// Unrecognized (nunca prêmio, nunca seguível). Persistido como string (ADR-031).
/// </summary>
public enum EQuotationResult
{
    ReadyForEmission,
    Analysis,
    Unavailable,
    Unrecognized,
}

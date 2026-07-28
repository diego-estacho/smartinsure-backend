namespace SmartInsure.Core.Enumerators;

/// <summary>
/// Classificação estável do resultado de uma Cotação (RN-058, ADR-064). Conjunto pequeno e fechado:
/// o motivo e a esteira acompanham a classificação como dado, não como status novo. Persistido como
/// string (ADR-031).
/// </summary>
public enum EQuotationResult
{
    /// <summary>Emissão automática disponível pela Seguradora.</summary>
    Automatic,

    /// <summary>Depende de esteira da Seguradora (ver <see cref="EAnalysisTrack"/>); segue no portal da Seguradora.</summary>
    Analysis,

    /// <summary>Seguradora não oferta, não pôde cotar ou recusou; acompanha a lista de motivos.</summary>
    Unavailable,

    /// <summary>Resultado que a plataforma não classificou; exibido sem prêmio, não seguível, registrado para revisão.</summary>
    Unrecognized,
}

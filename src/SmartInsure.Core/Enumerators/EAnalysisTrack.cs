namespace SmartInsure.Core.Enumerators;

/// <summary>
/// Esteira de análise de uma Cotação em <see cref="EQuotationResult.Analysis"/> (RN-058, ADR-064),
/// exposta por nome estável. O conjunto é completo (a Seguradora sempre atribui uma esteira específica);
/// nesta fase apenas a esteira de Subscrição é seguível (RN-059). Persistido como string (ADR-031).
/// </summary>
public enum EAnalysisTrack
{
    Underwriting,
    Credit,
    Pep,
    Reinsurance,
    Registration,
}

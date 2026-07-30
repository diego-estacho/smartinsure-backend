namespace SmartInsure.Core.Enumerators;

/// <summary>
/// Esteira específica de uma Cotação em Análise (RN-058, ADR-064): o corretor vê qual análise, nunca
/// um "requer análise" genérico. Apenas Underwriting (subscrição) é seguível nesta fase (RN-059).
/// Acompanha o resultado Analysis; persistido como string (ADR-031).
/// </summary>
public enum EAnalysisTrack
{
    Underwriting,
    Credit,
    Pep,
    Reinsurance,
    Registration,
}

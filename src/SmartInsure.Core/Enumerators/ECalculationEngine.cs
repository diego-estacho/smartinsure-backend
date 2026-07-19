namespace SmartInsure.Core.Enumerators;

/// <summary>
/// Motor de Cálculo (glossário, proposto 2026-07-19). Persistido como string (ADR-031).
/// RN-023: o motor de cada operação é resolvido pela Habilitação de Seguradora — nesta
/// fase o único motor disponível é o PlugV2.
/// </summary>
public enum ECalculationEngine
{
    PlugV2,
}

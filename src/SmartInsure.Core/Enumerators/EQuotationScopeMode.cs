namespace SmartInsure.Core.Enumerators;

/// <summary>
/// Escopo de Seguradoras a cotar de um Grupo de Cotação (RN-050). All = todas as habilitadas da
/// Corretora (direção do negócio — OPEN-07); Specific = subconjunto escolhido pelo corretor.
/// Persistido como string (ADR-031).
/// </summary>
public enum EQuotationScopeMode
{
    All,
    Specific,
}

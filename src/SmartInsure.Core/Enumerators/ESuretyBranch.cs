namespace SmartInsure.Core.Enumerators;

/// <summary>
/// Ramo regulatório do Seguro Garantia (glossário, proposto 2026-07-21). Persistido como string (ADR-031).
/// Trava do mapeamento: nenhum mapeamento cruza ramos (RN-035). No PlugV2, BranchCode 75 = Público, 76 = Privado.
/// </summary>
public enum ESuretyBranch
{
    Public,
    Private,
}

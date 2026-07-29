namespace SmartInsure.Core.Enumerators;

/// <summary>
/// Situação da Cobertura Adicional canônica (glossário, proposto 2026-07-23). Persistido como string
/// (ADR-031). Ativada/inativada pelo Administrador do Sistema na curadoria (RN-040). Nunca excluída.
/// </summary>
public enum EAdditionalCoverageStatus
{
    Active,
    Inactive,
}

namespace SmartInsure.Core.Enumerators;

/// <summary>
/// Situação da Cobertura Adicional Importada (glossário, proposto 2026-07-23). Persistido como string
/// (ADR-031). Passa a Inativa automaticamente quando some de uma consulta bem-sucedida da sua
/// Modalidade Importada (RN-044); reaparecendo, é reativada. Nunca excluída.
/// </summary>
public enum EImportedAdditionalCoverageStatus
{
    Active,
    Inactive,
}

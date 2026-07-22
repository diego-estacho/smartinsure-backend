namespace SmartInsure.Core.Enumerators;

/// <summary>
/// Forma de estabelecimento do Mapeamento de Modalidade (glossário, proposto 2026-07-21).
/// Persistido como string (ADR-031). Nesta fase só `Identifier` é automático (RN-032);
/// `Similarity` está fora de escopo (OPEN-08) e `Manual` é da Fila (RN-034, fatia 3).
/// </summary>
public enum EMappingEstablishment
{
    Identifier,
    Similarity,
    Manual,
}

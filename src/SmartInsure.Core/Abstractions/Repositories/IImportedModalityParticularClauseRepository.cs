using SmartInsure.Core.Entities;

namespace SmartInsure.Core.Abstractions.Repositories;

public interface IImportedModalityParticularClauseRepository : IRepository<ImportedModalityParticularClause>
{
    /// <summary>RN-041/042: cláusulas da Modalidade Importada (rastreadas) — upsert por chave externa e reconciliação.</summary>
    Task<IReadOnlyList<ImportedModalityParticularClause>> ListByImportedModalityAsync(
        Guid importedModalityId, CancellationToken cancellationToken);
}

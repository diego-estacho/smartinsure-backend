using SmartInsure.Core.Entities;

namespace SmartInsure.Core.Abstractions.Repositories;

public interface IImportedModalityTagRepository : IRepository<ImportedModalityTag>
{
    /// <summary>RN-040: reencontra a Tag pela Modalidade Importada (1:1), rastreada, para upsert.</summary>
    Task<ImportedModalityTag?> GetByImportedModalityAsync(Guid importedModalityId, CancellationToken cancellationToken);
}

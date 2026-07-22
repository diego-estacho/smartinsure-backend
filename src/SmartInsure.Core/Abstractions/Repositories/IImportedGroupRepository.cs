using SmartInsure.Core.Entities;

namespace SmartInsure.Core.Abstractions.Repositories;

public interface IImportedGroupRepository : IRepository<ImportedGroup>
{
    /// <summary>RN-030: reencontra o Grupo Importado pelo identificador de origem, por Seguradora (rastreado, para upsert).</summary>
    Task<ImportedGroup?> GetByInsurerAndSourceAsync(
        Guid insurerId, string sourceId, CancellationToken cancellationToken);
}

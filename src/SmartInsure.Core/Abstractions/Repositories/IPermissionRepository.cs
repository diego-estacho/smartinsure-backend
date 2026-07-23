using SmartInsure.Core.Entities;

namespace SmartInsure.Core.Abstractions.Repositories;

public interface IPermissionRepository : IRepository<Permission>
{
    /// <summary>Permissões do catálogo pelos seus códigos (RN-033).</summary>
    Task<IReadOnlyCollection<Permission>> GetByCodesAsync(
        IEnumerable<string> codes, CancellationToken cancellationToken);
}

using SmartInsure.Core.Entities;

namespace SmartInsure.Core.Abstractions.Repositories;

public interface IPermissionRepository : IRepository<Permission>
{
    /// <summary>Permissões do catálogo pelos seus códigos (RN-063).</summary>
    Task<IReadOnlyCollection<Permission>> GetByCodesAsync(
        IEnumerable<string> codes, CancellationToken cancellationToken);

    /// <summary>
    /// RN-063: o catálogo declarado pela plataforma, ordenado por código — é a lista oferecida na
    /// edição de qualquer Perfil, sem inclusão manual.
    /// </summary>
    Task<IReadOnlyList<Permission>> ListAllAsync(CancellationToken cancellationToken);
}

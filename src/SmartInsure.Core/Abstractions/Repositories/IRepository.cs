using SmartInsure.Core.Entities;

namespace SmartInsure.Core.Abstractions.Repositories;

/// <summary>
/// Contrato base de repositório por agregado (ADR-036). A conclusão da unidade de
/// trabalho é sempre do UseCase via <see cref="IUnitOfWork"/>.
/// </summary>
public interface IRepository<TEntity> where TEntity : EntityBase
{
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task AddAsync(TEntity entity, CancellationToken cancellationToken);

    void Update(TEntity entity);

    void Remove(TEntity entity);
}

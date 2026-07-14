using Microsoft.EntityFrameworkCore;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Entities;
using SmartInsure.Infra.Data.Context;

namespace SmartInsure.Infra.Data.Repositories;

/// <summary>
/// Base de repositório por agregado (ADR-036): compartilha o DbContext scoped do request
/// e nunca chama SaveChanges — o commit é do UseCase via IUnitOfWork. Leituras de
/// consulta usam AsNoTracking com projeção direta para DTOs (ADR-038).
/// </summary>
public abstract class Repository<TEntity>(SmartInsureDbContext context) : IRepository<TEntity>
    where TEntity : EntityBase
{
    protected SmartInsureDbContext Context { get; } = context;

    protected DbSet<TEntity> Set => Context.Set<TEntity>();

    public virtual async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => await Set.FindAsync([id], cancellationToken);

    public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken)
        => await Set.AddAsync(entity, cancellationToken);

    public virtual void Update(TEntity entity)
        => Set.Update(entity);

    public virtual void Remove(TEntity entity)
        => Set.Remove(entity);
}

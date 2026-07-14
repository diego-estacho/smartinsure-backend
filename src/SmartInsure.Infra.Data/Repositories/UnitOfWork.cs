using Microsoft.EntityFrameworkCore.Storage;
using SmartInsure.Core.Abstractions;
using SmartInsure.Infra.Data.Context;

namespace SmartInsure.Infra.Data.Repositories;

public sealed class UnitOfWork(SmartInsureDbContext context) : IUnitOfWork
{
    public Task<int> CommitAsync(CancellationToken cancellationToken)
        => context.SaveChangesAsync(cancellationToken);

    public async Task<ITransaction> BeginTransactionAsync(CancellationToken cancellationToken)
        => new EfTransaction(await context.Database.BeginTransactionAsync(cancellationToken));

    private sealed class EfTransaction(IDbContextTransaction transaction) : ITransaction
    {
        public Task CommitAsync(CancellationToken cancellationToken)
            => transaction.CommitAsync(cancellationToken);

        public Task RollbackAsync(CancellationToken cancellationToken)
            => transaction.RollbackAsync(cancellationToken);

        public ValueTask DisposeAsync()
            => transaction.DisposeAsync();
    }
}

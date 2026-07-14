namespace SmartInsure.Core.Abstractions;

/// <summary>
/// Unidade de trabalho (ADR-036): o UseCase decide o momento transacional;
/// repositórios nunca chamam SaveChanges.
/// </summary>
public interface IUnitOfWork
{
    Task<int> CommitAsync(CancellationToken cancellationToken);

    Task<ITransaction> BeginTransactionAsync(CancellationToken cancellationToken);
}

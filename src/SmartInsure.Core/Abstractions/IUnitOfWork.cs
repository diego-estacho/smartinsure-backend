namespace SmartInsure.Core.Abstractions;

/// <summary>
/// Unidade de trabalho (ADR-036): o UseCase decide o momento transacional;
/// repositórios nunca chamam SaveChanges.
/// </summary>
public interface IUnitOfWork
{
    Task<int> CommitAsync(CancellationToken cancellationToken);

    Task<ITransaction> BeginTransactionAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Descarta as mudanças ainda rastreadas pelo ChangeTracker sem persistir. Usado no isolamento
    /// por unidade (RN-038): após um rollback, garante que mudanças parciais não vazem para a próxima
    /// unidade que compartilha o mesmo DbContext escopado.
    /// </summary>
    void DiscardChanges();
}

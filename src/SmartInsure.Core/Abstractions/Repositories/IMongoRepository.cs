using System.Linq.Expressions;

namespace SmartInsure.Core.Abstractions.Repositories;

/// <summary>
/// Repositório genérico Mongo (ADR-039): exclusivo para dados não relacionais
/// (logs de integração, payloads de erro, auditoria de tarefas) — nunca agregados de negócio.
/// </summary>
public interface IMongoRepository<TDocument> where TDocument : class
{
    Task InsertAsync(TDocument document, CancellationToken cancellationToken);

    Task<IReadOnlyList<TDocument>> FindAsync(
        Expression<Func<TDocument, bool>> filter,
        CancellationToken cancellationToken);
}

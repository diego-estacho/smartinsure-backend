using System.Linq.Expressions;
using MongoDB.Driver;
using SmartInsure.Core.Abstractions.Repositories;

namespace SmartInsure.Infra.Data.Repositories;

/// <summary>
/// Repositório genérico Mongo (ADR-039): coleção nomeada pelo tipo do documento.
/// Exclusivo para dados não relacionais — nunca agregados de negócio.
/// </summary>
public sealed class MongoRepository<TDocument>(IMongoDatabase database) : IMongoRepository<TDocument>
    where TDocument : class
{
    private readonly IMongoCollection<TDocument> _collection =
        database.GetCollection<TDocument>(typeof(TDocument).Name);

    public Task InsertAsync(TDocument document, CancellationToken cancellationToken)
        => _collection.InsertOneAsync(document, options: null, cancellationToken);

    public async Task<IReadOnlyList<TDocument>> FindAsync(
        Expression<Func<TDocument, bool>> filter,
        CancellationToken cancellationToken)
        => await _collection.Find(filter).ToListAsync(cancellationToken);
}

using MongoDB.Driver;
using SmartInsure.Core.Observability;

namespace SmartInsure.Api.HostedServices;

/// <summary>
/// Garante, no startup da API (ADR-102), o índice TTL de <see cref="QuotationIntegrationLog"/> em
/// ExpiresAtUtc (expireAfterSeconds: 0) — a retenção do log de integração é responsabilidade do app, não
/// de migration/schema (Mongo não tem Flyway). Idempotente: <c>CreateOneAsync</c> com a mesma definição de
/// índice não duplica. Best-effort: índice ausente não pode impedir a API de subir.
/// </summary>
public sealed class MongoIndexInitializer(
    IMongoDatabase database,
    ILogger<MongoIndexInitializer> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            var collection = database.GetCollection<QuotationIntegrationLog>(nameof(QuotationIntegrationLog));

            var indexKeys = Builders<QuotationIntegrationLog>.IndexKeys.Ascending(log => log.ExpiresAtUtc);
            var indexModel = new CreateIndexModel<QuotationIntegrationLog>(
                indexKeys, new CreateIndexOptions { ExpireAfter = TimeSpan.Zero });

            await collection.Indexes.CreateOneAsync(indexModel, cancellationToken: cancellationToken);
        }
        catch (Exception exception)
        {
            // Best-effort no startup (ADR-102): índice ausente não deve impedir a API de subir.
            logger.LogWarning(exception, "Falha ao garantir o índice TTL de QuotationIntegrationLog no Mongo.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

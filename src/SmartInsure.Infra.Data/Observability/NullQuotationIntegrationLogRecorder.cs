using SmartInsure.Core.Abstractions.Services;

namespace SmartInsure.Infra.Data.Observability;

/// <summary>
/// No-op do log de integração (ADR-102) para hosts sem Mongo configurado (ex.: SmartInsure.Functions,
/// registerMongo:false — ver DependencyInjection). PlugV2CalculationEngine depende do recorder mesmo fora
/// do fluxo de Cotação (import de modalidades/coberturas); sem este fallback a resolução do motor via DI
/// quebraria nesses hosts. Fora de escopo do ADR-102 (que só normatiza o registro real, quando Mongo existe).
/// </summary>
public sealed class NullQuotationIntegrationLogRecorder : IQuotationIntegrationLogRecorder
{
    public Task RecordCotationAsync(QuotationIntegrationLogContext context, CancellationToken cancellationToken)
        => Task.CompletedTask;
}

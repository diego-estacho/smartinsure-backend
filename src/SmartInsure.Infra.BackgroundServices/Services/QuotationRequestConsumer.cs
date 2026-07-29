using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SmartInsure.Core.Abstractions;

namespace SmartInsure.Infra.BackgroundServices.Services;

/// <summary>
/// Consumidor do fan-out de cotação (RN-057, ADR-050): para cada item, resolve o processor no escopo
/// próprio e obtém+persiste a Cotação. Falha isolada de um item não derruba os demais (base WorkItemConsumer).
/// </summary>
public sealed class QuotationRequestConsumer(
    IQuotationRequestChannel channel,
    IServiceScopeFactory scopeFactory,
    ILogger<QuotationRequestConsumer> logger)
    : WorkItemConsumer<QuotationRequestWorkItem>(channel, scopeFactory, logger)
{
    protected override Task ProcessAsync(
        IServiceProvider services, QuotationRequestWorkItem workItem, CancellationToken cancellationToken)
    {
        var processor = services.GetRequiredService<IQuotationRequestProcessor>();
        return processor.ProcessAsync(workItem, cancellationToken);
    }
}

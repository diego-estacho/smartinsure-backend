using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartInsure.Core.Abstractions.Channels;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Infra.BackgroundServices.Options;

namespace SmartInsure.Infra.BackgroundServices.Services;

/// <summary>
/// Reconciliador do fan-out de cotação (ADR-050): varre as Cotações presas em Requested (perdidas em
/// restart/deploy no meio do processamento) e as reenfileira. O banco é o registro; a fila é otimização.
/// </summary>
public sealed class QuotationReconciler(
    IQuotationRequestChannel channel,
    IServiceScopeFactory scopeFactory,
    IOptions<QuotationFanOutOptions> options,
    ILogger<QuotationReconciler> logger)
    : PeriodicReconciler(TimeSpan.FromSeconds(options.Value.ReconcilerIntervalSeconds), scopeFactory, logger)
{
    private readonly int _staleAfterSeconds = options.Value.StaleRequestedAfterSeconds;

    protected override async Task ReconcileAsync(IServiceProvider services, CancellationToken cancellationToken)
    {
        var quotationRepository = services.GetRequiredService<IQuotationRepository>();

        var threshold = DateTime.UtcNow.AddSeconds(-_staleAfterSeconds);
        var stale = await quotationRepository.ListStaleRequestedAsync(threshold, cancellationToken);

        foreach (var quotation in stale)
        {
            await channel.EnqueueAsync(
                new QuotationRequestWorkItem(quotation.Id, quotation.QuotationGroupId, quotation.InsurerId),
                cancellationToken);
        }
    }
}

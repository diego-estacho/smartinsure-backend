using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;

namespace SmartInsure.Infra.BackgroundServices.Services;

/// <summary>
/// Reconciliador do fan-out de cotação (ADR-050): reenfileira as Cotações que ficaram em Requested após
/// restart/deploy — a fila in-process é volátil, o banco é o registro. O consumidor carimba o lease
/// (ProcessingStartedAt) antes de acionar o provedor; aqui só reenfileiramos as cujo lease expirou (ou que
/// nunca foram obtidas), nunca uma solicitação ainda em voo — a chamada NÃO é idempotente (cria proposta,
/// RN-057). Resíduo aceito: um crash entre a resposta do provedor e o commit ainda pode duplicar após o
/// lease expirar; eliminá-lo exigiria idempotência no provedor (fora de escopo — OPEN-07).
/// </summary>
public sealed class QuotationReconciler(
    IServiceScopeFactory scopeFactory,
    ILogger<QuotationReconciler> logger)
    : PeriodicReconciler(Interval, scopeFactory, logger)
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(1);

    // Lease: precisa ser > o teto de uma tentativa em voo (PlugV2 client timeout = 30s) + folga. Se aquele
    // timeout subir, este valor deve acompanhar, senão o reconciliador pode reenfileirar uma tentativa viva.
    private static readonly TimeSpan StaleAfter = TimeSpan.FromMinutes(2);

    protected override async Task ReconcileAsync(IServiceProvider services, CancellationToken cancellationToken)
    {
        var quotationRepository = services.GetRequiredService<IQuotationRepository>();
        var channel = services.GetRequiredService<IQuotationRequestChannel>();

        var stale = await quotationRepository.ListStaleRequestedWorkItemsAsync(
            DateTime.UtcNow - StaleAfter, cancellationToken);

        foreach (var workItem in stale)
        {
            await channel.EnqueueAsync(workItem, cancellationToken);
        }

        if (stale.Count > 0)
        {
            logger.LogInformation(
                "Reconciliador de cotação reenfileirou {Count} solicitação(ões) parada(s) em Requested.", stale.Count);
        }
    }
}

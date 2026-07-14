using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SmartInsure.Core.Abstractions.Channels;

namespace SmartInsure.Infra.BackgroundServices.Services;

/// <summary>
/// Consumidor base de work items (ADR-050): scope de DI próprio por item; falha de um
/// item nunca derruba o consumidor — o processamento é idempotente e o reconciliador
/// recupera itens perdidos.
/// </summary>
public abstract class WorkItemConsumer<TWorkItem>(
    IWorkItemChannel<TWorkItem> channel,
    IServiceScopeFactory scopeFactory,
    ILogger logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var workItem in channel.DequeueAllAsync(stoppingToken))
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                await ProcessAsync(scope.ServiceProvider, workItem, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                logger.LogError(
                    exception,
                    "Falha ao processar work item {WorkItemType}",
                    typeof(TWorkItem).Name);
            }
        }
    }

    protected abstract Task ProcessAsync(
        IServiceProvider services,
        TWorkItem workItem,
        CancellationToken cancellationToken);
}

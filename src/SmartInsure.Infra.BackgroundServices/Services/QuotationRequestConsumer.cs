using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartInsure.Core.Abstractions.Channels;
using SmartInsure.Infra.BackgroundServices.Options;

namespace SmartInsure.Infra.BackgroundServices.Services;

/// <summary>
/// Consumidor do fan-out de cotação (ADR-050): lê a fila e processa as Cotações com concorrência
/// limitada (SemaphoreSlim), scope de DI próprio por item. Uma falha nunca derruba o consumidor.
/// A concorrência é o que preserva a latência do corretor (as Seguradoras não são cotadas em série).
/// </summary>
public sealed class QuotationRequestConsumer(
    IQuotationRequestChannel channel,
    IServiceScopeFactory scopeFactory,
    IOptions<QuotationFanOutOptions> options,
    ILogger<QuotationRequestConsumer> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var throttler = new SemaphoreSlim(options.Value.MaxConcurrency);

        await foreach (var workItem in channel.DequeueAllAsync(stoppingToken))
        {
            await throttler.WaitAsync(stoppingToken);
            _ = ProcessAsync(workItem, throttler, stoppingToken);
        }
    }

    private async Task ProcessAsync(
        QuotationRequestWorkItem workItem, SemaphoreSlim throttler, CancellationToken stoppingToken)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var processor = scope.ServiceProvider.GetRequiredService<QuotationRequestProcessor>();
            await processor.ProcessAsync(workItem, stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Shutdown: encerra sem ruído.
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Falha ao processar a Cotação {QuotationId} (Grupo {GroupId}, Seguradora {InsurerId})",
                workItem.QuotationId, workItem.QuotationGroupId, workItem.InsurerId);
        }
        finally
        {
            throttler.Release();
        }
    }
}

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SmartInsure.Infra.BackgroundServices.Services;

/// <summary>
/// Reconciliador periódico base (ADR-050): varre o estado persistido e reenfileira
/// itens perdidos em restart/deploy. Todo processamento em memória tem um reconciliador —
/// a fila é otimização de latência, o banco é o registro.
/// </summary>
public abstract class PeriodicReconciler(
    TimeSpan interval,
    IServiceScopeFactory scopeFactory,
    ILogger logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(interval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                await ReconcileAsync(scope.ServiceProvider, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Falha no reconciliador {Reconciler}", GetType().Name);
            }
        }
    }

    protected abstract Task ReconcileAsync(
        IServiceProvider services,
        CancellationToken cancellationToken);
}

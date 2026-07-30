using Microsoft.Extensions.DependencyInjection;
using SmartInsure.Core.Abstractions;
using SmartInsure.Infra.BackgroundServices.Channels;
using SmartInsure.Infra.BackgroundServices.Services;

namespace SmartInsure.Infra.BackgroundServices;

public static class DependencyInjection
{
    /// <summary>
    /// Registro dos pares channel + consumidor + reconciliador (ADR-050). Fan-out de cotação (RN-056/057):
    /// canal in-process singleton (compartilhado entre o enfileirador da API e o consumidor) + consumidor
    /// hosted + reconciliador que reenfileira as Cotações paradas em Requested após restart (a fila é volátil).
    /// </summary>
    public static IServiceCollection AddBackgroundServices(this IServiceCollection services)
    {
        services.AddSingleton<IQuotationRequestChannel, QuotationRequestChannel>();
        services.AddHostedService<QuotationRequestConsumer>();
        services.AddHostedService<QuotationReconciler>();

        return services;
    }
}

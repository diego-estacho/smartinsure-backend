using Microsoft.Extensions.DependencyInjection;
using SmartInsure.Core.Abstractions;
using SmartInsure.Infra.BackgroundServices.Channels;
using SmartInsure.Infra.BackgroundServices.Services;

namespace SmartInsure.Infra.BackgroundServices;

public static class DependencyInjection
{
    /// <summary>
    /// Registro dos pares channel + consumidor (ADR-050). Fan-out de cotação (RN-056/057): canal
    /// in-process singleton (compartilhado entre o enfileirador da API e o consumidor) + consumidor hosted.
    /// </summary>
    public static IServiceCollection AddBackgroundServices(this IServiceCollection services)
    {
        services.AddSingleton<IQuotationRequestChannel, QuotationRequestChannel>();
        services.AddHostedService<QuotationRequestConsumer>();

        return services;
    }
}

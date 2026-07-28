using Microsoft.Extensions.DependencyInjection;
using SmartInsure.Core.Abstractions.Channels;
using SmartInsure.Infra.BackgroundServices.Channels;
using SmartInsure.Infra.BackgroundServices.Options;
using SmartInsure.Infra.BackgroundServices.Services;

namespace SmartInsure.Infra.BackgroundServices;

public static class DependencyInjection
{
    /// <summary>
    /// Registro dos pares channel + consumidor + reconciliador (ADR-050).
    /// </summary>
    public static IServiceCollection AddBackgroundServices(this IServiceCollection services)
    {
        // ADR-053: travas do fan-out configuráveis com default (config ausente usa os defaults).
        services.AddOptions<QuotationFanOutOptions>()
            .BindConfiguration(QuotationFanOutOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // Fan-out de cotação (RN-056/057): fila singleton + consumidor + reconciliador.
        services.AddSingleton<QuotationRequestChannel>();
        services.AddSingleton<IQuotationRequestChannel>(
            provider => provider.GetRequiredService<QuotationRequestChannel>());
        services.AddScoped<QuotationRequestProcessor>();
        services.AddHostedService<QuotationRequestConsumer>();
        services.AddHostedService<QuotationReconciler>();

        return services;
    }
}

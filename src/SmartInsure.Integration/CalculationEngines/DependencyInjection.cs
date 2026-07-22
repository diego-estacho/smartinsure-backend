using Microsoft.Extensions.DependencyInjection;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Enumerators;
using SmartInsure.Integration.CalculationEngines.PlugV2;
using SmartInsure.Integration.CalculationEngines.Services;

namespace SmartInsure.Integration.CalculationEngines;

public static class DependencyInjection
{
    public static IServiceCollection AddCalculationEngines(this IServiceCollection services)
    {
        // RN-023: motores registrados por chave do enum — a escolha em runtime é sempre
        // da Habilitação de Seguradora, via ICalculationEngineResolver. A conexão
        // (baseUrl/key) é por vínculo (ConnectionParameters), não configuração global.
        services.AddKeyedScoped<ICalculationEngine, PlugV2CalculationEngine>(ECalculationEngine.PlugV2);

        services.AddScoped<ICalculationEngineResolver, CalculationEngineResolver>();

        // RN-031/ADR-044: base URL por Habilitação (montada por chamada), resiliência no client nomeado.
        services.AddHttpClient(PlugV2ModalityImportClient.HttpClientName)
            .AddStandardResilienceHandler();
        services.AddScoped<PlugV2ModalityImportClient>();

        return services;
    }
}

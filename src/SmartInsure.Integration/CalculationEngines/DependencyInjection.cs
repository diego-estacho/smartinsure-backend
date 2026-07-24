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
        // RN-029: cliente HTTP PlugV2 com resiliência padrão para consultas de crédito e demais operações.
        services.AddHttpClient("PlugV2")
            .AddStandardResilienceHandler();

        // RN-023: motores registrados por chave do enum — a escolha em runtime é sempre
        // da Habilitação de Seguradora, via ICalculationEngineResolver. A conexão
        // (baseUrl/key) é por vínculo (ConnectionParameters), não configuração global.
        services.AddKeyedScoped<ICalculationEngine, PlugV2CalculationEngine>(ECalculationEngine.PlugV2);

        services.AddScoped<ICalculationEngineResolver, CalculationEngineResolver>();

        // RN-034/ADR-044: base URL por Habilitação (montada por chamada), resiliência no client nomeado.
        services.AddHttpClient(PlugV2ModalityImportClient.HttpClientName)
            .AddStandardResilienceHandler();
        services.AddScoped<PlugV2ModalityImportClient>();

        // RN-047/ADR-044: idem para GetModalityObject — base URL por Habilitação, resiliência no client nomeado.
        services.AddHttpClient(PlugV2ModalityObjectClient.HttpClientName)
            .AddStandardResilienceHandler();
        services.AddScoped<PlugV2ModalityObjectClient>();

        // RN-042/RN-044/ADR-044: base URL por Habilitação (montada por chamada), resiliência no client nomeado.
        services.AddHttpClient(PlugV2AdditionalCoveragesClient.HttpClientName)
            .AddStandardResilienceHandler();
        services.AddScoped<PlugV2AdditionalCoveragesClient>();

        return services;
    }
}

using Microsoft.Extensions.DependencyInjection;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Enumerators;
using SmartInsure.Integration.CalculationEngines.Options;
using SmartInsure.Integration.CalculationEngines.Services;

namespace SmartInsure.Integration.CalculationEngines;

public static class DependencyInjection
{
    public static IServiceCollection AddCalculationEngines(this IServiceCollection services)
    {
        services.AddOptions<PlugV2Options>()
            .BindConfiguration(PlugV2Options.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // RN-023: motores registrados por chave do enum — a escolha em runtime é sempre
        // da Habilitação de Seguradora, via ICalculationEngineResolver.
        services.AddKeyedScoped<ICalculationEngine, PlugV2CalculationEngine>(ECalculationEngine.PlugV2);

        services.AddScoped<ICalculationEngineResolver, CalculationEngineResolver>();

        return services;
    }
}

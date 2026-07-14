using Microsoft.Extensions.DependencyInjection;
using SmartInsure.Infra.CrossCutting.Options;

namespace SmartInsure.Infra.CrossCutting.IoC;

public static class DependencyInjection
{
    public static IServiceCollection AddCrossCutting(this IServiceCollection services)
    {
        // ADR-053: toda Options com BindConfiguration + ValidateOnStart — config ausente falha o startup.
        services.AddOptions<JwtOptions>()
            .BindConfiguration(JwtOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }
}

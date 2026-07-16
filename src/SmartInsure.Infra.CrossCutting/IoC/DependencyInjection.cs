using Microsoft.Extensions.DependencyInjection;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Infra.CrossCutting.Identity;
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

        // RN-005: emissão do acesso autenticado com a mesma chave simétrica da validação (ADR-015).
        services.AddSingleton<IAccessTokenIssuer, JwtAccessTokenIssuer>();

        // RN-006: denylist de acessos encerrados sobre o cache distribuído (ADR-040).
        services.AddSingleton<IAccessTokenRevocationStore, CacheAccessTokenRevocationStore>();

        return services;
    }
}

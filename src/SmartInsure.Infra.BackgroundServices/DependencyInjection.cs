using Microsoft.Extensions.DependencyInjection;

namespace SmartInsure.Infra.BackgroundServices;

public static class DependencyInjection
{
    /// <summary>
    /// Ponto de registro dos pares channel + consumidor + reconciliador (ADR-050),
    /// preenchido conforme as operações assíncronas forem criadas.
    /// </summary>
    public static IServiceCollection AddBackgroundServices(this IServiceCollection services)
        => services;
}

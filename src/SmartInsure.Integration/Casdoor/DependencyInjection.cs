using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Refit;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Integration.Casdoor.Interfaces;
using SmartInsure.Integration.Casdoor.Options;
using SmartInsure.Integration.Casdoor.Services;

namespace SmartInsure.Integration.Casdoor;

public static class DependencyInjection
{
    public static IServiceCollection AddCasdoorIntegration(this IServiceCollection services)
    {
        services.AddOptions<CasdoorOptions>()
            .BindConfiguration(CasdoorOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // ADR-044: resiliência universal em toda chamada HTTP de saída.
        services.AddRefitClient<ICasdoorApi>()
            .ConfigureHttpClient((provider, client) =>
            {
                var options = provider.GetRequiredService<IOptions<CasdoorOptions>>().Value;

                client.BaseAddress = new Uri(options.Domain);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                    "Basic",
                    Convert.ToBase64String(Encoding.UTF8.GetBytes($"{options.ClientId}:{options.Secret}")));
            })
            .AddStandardResilienceHandler();

        services.AddScoped<IIdentityProvider, CasdoorIdentityProvider>();

        return services;
    }
}

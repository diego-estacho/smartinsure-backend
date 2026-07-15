using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Refit;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Integration.Bureau.Interfaces;
using SmartInsure.Integration.Bureau.Options;
using SmartInsure.Integration.Bureau.Services;

namespace SmartInsure.Integration.Bureau;

public static class DependencyInjection
{
    public static IServiceCollection AddBureauIntegration(this IServiceCollection services)
    {
        services.AddOptions<BureauOptions>()
            .BindConfiguration(BureauOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // ADR-044: resiliência universal em toda chamada HTTP de saída.
        // ADR-050: só operação idempotente recebe retry; GetPersonComplement é POST por
        // contrato do gateway, porém leitura pura — retry seguro.
        services.AddRefitClient<IBureauApi>()
            .ConfigureHttpClient((provider, client) =>
            {
                var options = provider.GetRequiredService<IOptions<BureauOptions>>().Value;

                client.BaseAddress = new Uri(options.BaseUrl);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                    "Basic",
                    Convert.ToBase64String(Encoding.UTF8.GetBytes($"{options.UserName}:{options.Password}")));
                client.DefaultRequestHeaders.Add("InsuranceCompanyId", options.InsuranceCompanyId);
                client.DefaultRequestHeaders.Add("Product", options.Product);
            })
            .AddStandardResilienceHandler();

        services.AddScoped<IBureauProvider, BureauProvider>();

        return services;
    }
}

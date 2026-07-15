using Microsoft.Extensions.DependencyInjection;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.MailServices.Options;
using SmartInsure.MailServices.Services;

namespace SmartInsure.MailServices;

public static class DependencyInjection
{
    public static IServiceCollection AddMailServices(this IServiceCollection services)
    {
        services.AddOptions<MailOptions>()
            .BindConfiguration(MailOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddScoped<IMailService, MailKitMailService>();

        return services;
    }
}

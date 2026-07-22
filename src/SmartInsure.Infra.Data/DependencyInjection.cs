using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Infra.Data.Context;
using SmartInsure.Infra.Data.Options;
using SmartInsure.Infra.Data.Repositories;

namespace SmartInsure.Infra.Data;

public static class DependencyInjection
{
    public static IServiceCollection AddInfraData(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ICurrentUserAccessor é opcional por design (ADR-035): ausente = execução de sistema.
        services.AddScoped(provider =>
            new AuditSaveChangesInterceptor(provider.GetService<ICurrentUserAccessor>()));

        services.AddDbContext<SmartInsureDbContext>((provider, options) =>
            options.UseSqlServer(configuration.GetConnectionString("SqlServer"))
                .AddInterceptors(provider.GetRequiredService<AuditSaveChangesInterceptor>()));

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IInsurerRepository, InsurerRepository>();
        services.AddScoped<IPersonRepository, PersonRepository>();
        services.AddScoped<ILegalNatureRepository, LegalNatureRepository>();
        services.AddScoped<IBrokerageInsurerEnablementRepository, BrokerageInsurerEnablementRepository>();
        services.AddScoped<IPolicyHolderAppointmentRepository, PolicyHolderAppointmentRepository>();
        services.AddScoped<IModalityRepository, ModalityRepository>();
        services.AddScoped<IImportedGroupRepository, ImportedGroupRepository>();
        services.AddScoped<IImportedModalityRepository, ImportedModalityRepository>();

        services.AddOptions<MongoOptions>()
            .BindConfiguration(MongoOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<IMongoClient>(provider =>
            new MongoClient(provider.GetRequiredService<IOptions<MongoOptions>>().Value.ConnectionString));

        services.AddSingleton(provider =>
            provider.GetRequiredService<IMongoClient>()
                .GetDatabase(provider.GetRequiredService<IOptions<MongoOptions>>().Value.Database));

        services.AddScoped(typeof(IMongoRepository<>), typeof(MongoRepository<>));

        return services;
    }
}

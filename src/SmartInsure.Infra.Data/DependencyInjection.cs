using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Infra.Data.Context;
using SmartInsure.Infra.Data.Observability;
using SmartInsure.Infra.Data.Options;
using SmartInsure.Infra.Data.Repositories;

namespace SmartInsure.Infra.Data;

public static class DependencyInjection
{
    public static IServiceCollection AddInfraData(
        this IServiceCollection services,
        IConfiguration configuration,
        bool registerMongo = true)
    {
        // ICurrentUserAccessor é opcional por design (ADR-035): ausente = execução de sistema.
        services.AddScoped(provider =>
            new AuditSaveChangesInterceptor(provider.GetService<ICurrentUserAccessor>()));

        services.AddDbContext<SmartInsureDbContext>((provider, options) =>
            options.UseSqlServer(configuration.GetConnectionString("SqlServer"))
                .AddInterceptors(provider.GetRequiredService<AuditSaveChangesInterceptor>()));

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IProfileRepository, ProfileRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<IUserBrokerageMembershipRepository, UserBrokerageMembershipRepository>();
        services.AddScoped<IUserPolicyHolderMembershipRepository, UserPolicyHolderMembershipRepository>();
        services.AddScoped<IInvitationRepository, InvitationRepository>();
        services.AddScoped<IInsurerRepository, InsurerRepository>();
        services.AddScoped<IPersonRepository, PersonRepository>();
        services.AddScoped<ILegalNatureRepository, LegalNatureRepository>();
        services.AddScoped<IBrokerageInsurerEnablementRepository, BrokerageInsurerEnablementRepository>();
        services.AddScoped<IPolicyHolderAppointmentRepository, PolicyHolderAppointmentRepository>();
        services.AddScoped<IModalityRepository, ModalityRepository>();
        services.AddScoped<IImportedGroupRepository, ImportedGroupRepository>();
        services.AddScoped<IImportedModalityRepository, ImportedModalityRepository>();
        services.AddScoped<IImportedModalityTagRepository, ImportedModalityTagRepository>();
        services.AddScoped<IImportedModalityParticularClauseRepository, ImportedModalityParticularClauseRepository>();
        services.AddScoped<IAdditionalCoverageRepository, AdditionalCoverageRepository>();
        services.AddScoped<IImportedAdditionalCoverageRepository, ImportedAdditionalCoverageRepository>();
        services.AddScoped<ICreditInquiryRepository, CreditInquiryRepository>();
        services.AddScoped<IQuotationGroupRepository, QuotationGroupRepository>();
        services.AddScoped<IQuotationRepository, QuotationRepository>();

        // RN-506: Termo da Seguradora e o registro do aceite, exigidos para emitir.
        services.AddScoped<IInsurerTermRepository, InsurerTermRepository>();
        services.AddScoped<ITermAcceptanceRepository, TermAcceptanceRepository>();

        // RN-514: Apólice — registro da emissão solicitada.
        services.AddScoped<IPolicyRepository, PolicyRepository>();

        // Mongo é opcional por host: a API valida na inicialização (MongoOptions [Required] +
        // ValidateOnStart), mas o job de importação (SmartInsure.Functions) não usa Mongo — passa
        // registerMongo:false para não exigir config de Mongo só para bootar o host.
        if (registerMongo)
        {
            // MongoDB.Driver 3.x: o default de GuidRepresentation é Unspecified e serializar POCO com Guid
            // lança. Registramos Standard uma vez no startup (idempotente) — sem isso o QuotationIntegrationLog
            // (3 Guids) falha no insert e, por ser best-effort, some silenciosamente (ADR-102).
            BsonSerializer.TryRegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

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

            // ADR-102: log de integração da Cotação PlugV2 — primeiro consumidor real do IMongoRepository<>.
            services.AddScoped<IQuotationIntegrationLogRecorder, QuotationIntegrationLogRecorder>();
        }
        else
        {
            // PlugV2CalculationEngine depende do recorder mesmo fora do fluxo de Cotação (import de
            // modalidades/coberturas, SmartInsure.Functions) — no-op quando o host não registra Mongo
            // (registerMongo:false), senão a resolução do motor via DI quebra por dependência ausente.
            services.AddScoped<IQuotationIntegrationLogRecorder, NullQuotationIntegrationLogRecorder>();
        }

        return services;
    }
}

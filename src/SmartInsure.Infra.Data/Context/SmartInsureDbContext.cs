using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using SmartInsure.Core.Entities;

namespace SmartInsure.Infra.Data.Context;

/// <summary>
/// DbContext único da solution, atuando como Unit of Work (ADR-023, ADR-036).
/// Mapeamento 100% Fluent API por assembly scan (ADR-033); schema é do Flyway (ADR-041) —
/// EF Migrations proibidas.
/// </summary>
public sealed class SmartInsureDbContext(DbContextOptions<SmartInsureDbContext> options)
    : DbContext(options)
{
    public DbSet<Profile> Profiles => Set<Profile>();

    public DbSet<Permission> Permissions => Set<Permission>();

    public DbSet<ProfilePermission> ProfilePermissions => Set<ProfilePermission>();

    public DbSet<UserBrokerageMembership> UserBrokerageMemberships => Set<UserBrokerageMembership>();

    public DbSet<UserPolicyHolderMembership> UserPolicyHolderMemberships => Set<UserPolicyHolderMembership>();

    public DbSet<Invitation> Invitations => Set<Invitation>();

    public DbSet<Insurer> Insurers => Set<Insurer>();

    public DbSet<Person> Persons => Set<Person>();

    public DbSet<LegalNature> LegalNatures => Set<LegalNature>();

    public DbSet<BrokerageInsurerEnablement> BrokerageInsurerEnablements => Set<BrokerageInsurerEnablement>();

    public DbSet<Modality> Modalities => Set<Modality>();

    public DbSet<ImportedGroup> ImportedGroups => Set<ImportedGroup>();

    public DbSet<ImportedModality> ImportedModalities => Set<ImportedModality>();

    public DbSet<ImportedModalityTag> ImportedModalityTags => Set<ImportedModalityTag>();

    public DbSet<ImportedModalityParticularClause> ImportedModalityParticularClauses
        => Set<ImportedModalityParticularClause>();

    public DbSet<AdditionalCoverage> AdditionalCoverages => Set<AdditionalCoverage>();

    public DbSet<ImportedAdditionalCoverage> ImportedAdditionalCoverages => Set<ImportedAdditionalCoverage>();

    public DbSet<CreditInquiry> CreditInquiries => Set<CreditInquiry>();

    public DbSet<CreditInquiryResult> CreditInquiryResults => Set<CreditInquiryResult>();

    public DbSet<QuotationGroup> QuotationGroups => Set<QuotationGroup>();

    public DbSet<QuotationGroupInsurer> QuotationGroupInsurers => Set<QuotationGroupInsurer>();

    public DbSet<Quotation> Quotations => Set<Quotation>();

    public DbSet<QuotationReason> QuotationReasons => Set<QuotationReason>();

    public DbSet<QuotationGroupAdditionalCoverage> QuotationGroupAdditionalCoverages
        => Set<QuotationGroupAdditionalCoverage>();

    public DbSet<QuotationAdditionalCoverage> QuotationAdditionalCoverages
        => Set<QuotationAdditionalCoverage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SmartInsureDbContext).Assembly);

        // ADR-029/030: a identidade (UUIDv7) é gerada pela aplicação (EntityBase), nunca pelo banco.
        // Sem ValueGeneratedNever, adicionar um filho a um agregado já rastreado gera UPDATE (0 linhas)
        // em vez de INSERT — a chave client-side leva o EF a tratar o filho novo como já existente.
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var idProperty = entityType.FindProperty(nameof(EntityBase.Id));
            if (idProperty is not null && idProperty.ClrType == typeof(Guid))
            {
                idProperty.ValueGenerated = ValueGenerated.Never;
            }
        }

        // ADR-034: toda FK nasce Restrict; cascade delete nunca é habilitado.
        foreach (var foreignKey in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
        {
            foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
        }
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // ADR-031: enum persistido como string, por convenção global.
        configurationBuilder.Properties<Enum>().HaveConversion<string>();
    }
}

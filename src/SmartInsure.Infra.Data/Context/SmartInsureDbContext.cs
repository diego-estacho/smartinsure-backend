using Microsoft.EntityFrameworkCore;
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

    public DbSet<CreditInquiry> CreditInquiries => Set<CreditInquiry>();

    public DbSet<CreditInquiryResult> CreditInquiryResults => Set<CreditInquiryResult>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SmartInsureDbContext).Assembly);

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

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
    public DbSet<Insurer> Insurers => Set<Insurer>();

    public DbSet<Person> Persons => Set<Person>();

    public DbSet<LegalNature> LegalNatures => Set<LegalNature>();

    public DbSet<InsurerEnablement> InsurerEnablements => Set<InsurerEnablement>();

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

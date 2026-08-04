using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInsure.Core.Entities;

namespace SmartInsure.Infra.Data.Mappings;

/// <summary>Mapeamento da Cobertura Adicional canônica escolhida num Grupo de Cotação (RN-104).</summary>
public sealed class QuotationGroupAdditionalCoverageMapping
    : IEntityTypeConfiguration<QuotationGroupAdditionalCoverage>
{
    public void Configure(EntityTypeBuilder<QuotationGroupAdditionalCoverage> builder)
    {
        builder.ToTable("QuotationGroupAdditionalCoverages");

        builder.HasKey(coverage => coverage.Id);

        // Id gerado pela aplicação (EntityBase) — new-in-collection precisa virar INSERT, não UPDATE.
        builder.Property(coverage => coverage.Id).ValueGeneratedNever();

        builder.Property(coverage => coverage.QuotationGroupId).IsRequired();
        builder.Property(coverage => coverage.AdditionalCoverageId).IsRequired();

        // RN-104: uma Cobertura Adicional aparece uma única vez por Grupo de Cotação.
        builder.HasIndex(coverage => new { coverage.QuotationGroupId, coverage.AdditionalCoverageId })
            .IsUnique();

        // FK com DeleteBehavior.Restrict (convenção global, ADR-034).
        builder.HasOne<AdditionalCoverage>()
            .WithMany()
            .HasForeignKey(coverage => coverage.AdditionalCoverageId);

        builder.Property(coverage => coverage.CreatedAt).IsRequired();
        builder.Property(coverage => coverage.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(coverage => coverage.UpdatedBy).HasMaxLength(100);
    }
}

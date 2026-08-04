using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInsure.Core.Entities;

namespace SmartInsure.Infra.Data.Mappings;

/// <summary>
/// Mapeamento da situação de uma Cobertura Adicional dentro de uma Cotação (RN-105/RN-106).
/// </summary>
public sealed class QuotationAdditionalCoverageMapping : IEntityTypeConfiguration<QuotationAdditionalCoverage>
{
    public void Configure(EntityTypeBuilder<QuotationAdditionalCoverage> builder)
    {
        builder.ToTable("QuotationAdditionalCoverages");

        builder.HasKey(coverage => coverage.Id);

        // Id gerado pela aplicação (EntityBase) — new-in-collection precisa virar INSERT, não UPDATE.
        builder.Property(coverage => coverage.Id).ValueGeneratedNever();

        builder.Property(coverage => coverage.QuotationId).IsRequired();
        builder.Property(coverage => coverage.AdditionalCoverageId).IsRequired();

        // Enum como string por convenção global (ADR-031); nome estável no contrato.
        builder.Property(coverage => coverage.Status).HasMaxLength(20).IsRequired();

        // RN-105: casa com ImportedAdditionalCoverages.Name (300).
        builder.Property(coverage => coverage.SentName).HasMaxLength(300);

        // RN-106: uma situação por (Cotação, Cobertura Adicional escolhida).
        builder.HasIndex(coverage => new { coverage.QuotationId, coverage.AdditionalCoverageId })
            .IsUnique();

        builder.HasOne<AdditionalCoverage>()
            .WithMany()
            .HasForeignKey(coverage => coverage.AdditionalCoverageId);

        // Rastro da Importada de origem, quando identificável (nulo quando os ramos compartilham o nome).
        builder.HasOne<ImportedAdditionalCoverage>()
            .WithMany()
            .HasForeignKey(coverage => coverage.ImportedAdditionalCoverageId);

        builder.Property(coverage => coverage.CreatedAt).IsRequired();
        builder.Property(coverage => coverage.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(coverage => coverage.UpdatedBy).HasMaxLength(100);
    }
}

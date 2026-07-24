using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInsure.Core.Entities;

namespace SmartInsure.Infra.Data.Mappings;

public sealed class ImportedAdditionalCoverageMapping : IEntityTypeConfiguration<ImportedAdditionalCoverage>
{
    public void Configure(EntityTypeBuilder<ImportedAdditionalCoverage> builder)
    {
        builder.ToTable("ImportedAdditionalCoverages");

        builder.HasKey(coverage => coverage.Id);

        builder.Property(coverage => coverage.ImportedModalityId).IsRequired();
        builder.Property(coverage => coverage.Name).HasMaxLength(300).IsRequired();
        builder.Property(coverage => coverage.SourceUniqueId).HasMaxLength(100);
        builder.Property(coverage => coverage.InsuredAmountCalculationType).IsRequired();
        builder.Property(coverage => coverage.AllowManualEdit).IsRequired();
        builder.Property(coverage => coverage.IsIgnored).IsRequired();
        builder.Property(coverage => coverage.Status).HasMaxLength(20).IsRequired();
        builder.Property(coverage => coverage.LastImportedAt).IsRequired();

        // RN-041: identidade e reencontro do upsert por (Modalidade Importada, nome).
        builder.HasIndex(coverage => new { coverage.ImportedModalityId, coverage.Name }).IsUnique();
        builder.HasIndex(coverage => coverage.AdditionalCoverageId);

        builder.HasOne<ImportedModality>()
            .WithMany()
            .HasForeignKey(coverage => coverage.ImportedModalityId);

        // RN-043: vínculo manual com a Cobertura Adicional canônica (nulo = pendente).
        builder.HasOne<AdditionalCoverage>()
            .WithMany()
            .HasForeignKey(coverage => coverage.AdditionalCoverageId);

        builder.Property(coverage => coverage.CreatedAt).IsRequired();
        builder.Property(coverage => coverage.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(coverage => coverage.UpdatedBy).HasMaxLength(100);
    }
}

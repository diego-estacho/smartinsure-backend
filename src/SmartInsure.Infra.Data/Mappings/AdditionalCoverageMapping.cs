using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInsure.Core.Entities;

namespace SmartInsure.Infra.Data.Mappings;

public sealed class AdditionalCoverageMapping : IEntityTypeConfiguration<AdditionalCoverage>
{
    public void Configure(EntityTypeBuilder<AdditionalCoverage> builder)
    {
        builder.ToTable("AdditionalCoverages");

        builder.HasKey(coverage => coverage.Id);

        builder.Property(coverage => coverage.Name).HasMaxLength(300).IsRequired();
        builder.Property(coverage => coverage.Status).HasMaxLength(20).IsRequired();

        // RN-040: nome canônico único no catálogo.
        builder.HasIndex(coverage => coverage.Name).IsUnique();

        builder.Property(coverage => coverage.CreatedAt).IsRequired();
        builder.Property(coverage => coverage.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(coverage => coverage.UpdatedBy).HasMaxLength(100);
    }
}

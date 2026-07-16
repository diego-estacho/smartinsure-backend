using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInsure.Core.Entities;

namespace SmartInsure.Infra.Data.Mappings;

public sealed class LegalNatureMapping : IEntityTypeConfiguration<LegalNature>
{
    public void Configure(EntityTypeBuilder<LegalNature> builder)
    {
        builder.ToTable("LegalNatures");

        builder.HasKey(nature => nature.Id);

        builder.Property(nature => nature.EmissionYear).IsRequired();

        builder.Property(nature => nature.Code)
            .HasMaxLength(10)
            .IsRequired();

        // RN-015: um código CONCLA, uma natureza.
        builder.HasIndex(nature => nature.Code).IsUnique();

        builder.Property(nature => nature.Name)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(nature => nature.IsPrivate).IsRequired();

        // Alinhado 1:1 com a migration criar-tabela-legal-natures (evitar drift de constraint).
        builder.Property(nature => nature.CreatedAt).IsRequired();
        builder.Property(nature => nature.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(nature => nature.UpdatedBy).HasMaxLength(100);
    }
}

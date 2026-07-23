using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInsure.Core.Entities;

namespace SmartInsure.Infra.Data.Mappings;

public sealed class ImportedModalityTagMapping : IEntityTypeConfiguration<ImportedModalityTag>
{
    public void Configure(EntityTypeBuilder<ImportedModalityTag> builder)
    {
        builder.ToTable("ImportedModalityTags");
        builder.HasKey(tag => tag.Id);

        builder.Property(tag => tag.ImportedModalityId).IsRequired();
        builder.Property(tag => tag.JsonTag).IsRequired();
        builder.Property(tag => tag.ObjectText);
        builder.Property(tag => tag.Status).HasMaxLength(20).IsRequired();

        // RN-040: 1:1 com a Modalidade Importada.
        builder.HasIndex(tag => tag.ImportedModalityId).IsUnique();
        builder.HasOne<ImportedModality>().WithMany().HasForeignKey(tag => tag.ImportedModalityId);

        builder.Property(tag => tag.CreatedAt).IsRequired();
        builder.Property(tag => tag.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(tag => tag.UpdatedBy).HasMaxLength(100);
    }
}

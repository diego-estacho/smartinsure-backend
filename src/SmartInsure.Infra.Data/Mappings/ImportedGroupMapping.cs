using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInsure.Core.Entities;

namespace SmartInsure.Infra.Data.Mappings;

public sealed class ImportedGroupMapping : IEntityTypeConfiguration<ImportedGroup>
{
    public void Configure(EntityTypeBuilder<ImportedGroup> builder)
    {
        builder.ToTable("ImportedGroups");

        builder.HasKey(group => group.Id);

        builder.Property(group => group.InsurerId).IsRequired();
        builder.Property(group => group.SourceId).HasMaxLength(100).IsRequired();
        builder.Property(group => group.Name).HasMaxLength(200).IsRequired();
        builder.Property(group => group.Type).HasMaxLength(100);

        // RN-030: reencontro por (Seguradora, identificador de origem). 1:1 com a migration.
        builder.HasIndex(group => new { group.InsurerId, group.SourceId }).IsUnique();

        builder.Property(group => group.CreatedAt).IsRequired();
        builder.Property(group => group.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(group => group.UpdatedBy).HasMaxLength(100);
    }
}

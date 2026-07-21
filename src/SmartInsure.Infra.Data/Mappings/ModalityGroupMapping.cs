using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInsure.Core.Entities;

namespace SmartInsure.Infra.Data.Mappings;

public sealed class ModalityGroupMapping : IEntityTypeConfiguration<ModalityGroup>
{
    public void Configure(EntityTypeBuilder<ModalityGroup> builder)
    {
        builder.ToTable("ModalityGroups");

        builder.HasKey(group => group.Id);

        builder.Property(group => group.Name)
            .HasMaxLength(200)
            .IsRequired();

        // RN-029: nome do Grupo único no catálogo.
        builder.HasIndex(group => group.Name).IsUnique();

        builder.Property(group => group.Description)
            .HasMaxLength(1000);

        builder.Property(group => group.Status)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(group => group.DisplayOrder)
            .IsRequired();

        // Alinhado 1:1 com a migration criar-tabelas-modality-catalog (evitar drift de constraint).
        builder.Property(group => group.CreatedAt).IsRequired();
        builder.Property(group => group.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(group => group.UpdatedBy).HasMaxLength(100);
    }
}

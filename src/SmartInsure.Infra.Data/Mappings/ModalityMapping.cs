using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInsure.Core.Entities;

namespace SmartInsure.Infra.Data.Mappings;

public sealed class ModalityMapping : IEntityTypeConfiguration<Modality>
{
    public void Configure(EntityTypeBuilder<Modality> builder)
    {
        builder.ToTable("Modalities");

        builder.HasKey(modality => modality.Id);

        builder.Property(modality => modality.Name)
            .HasMaxLength(200)
            .IsRequired();

        // RN-032: nome da Modalidade único no catálogo.
        builder.HasIndex(modality => modality.Name).IsUnique();

        builder.Property(modality => modality.GlobalModalityExternalId)
            .HasMaxLength(100);

        // RN-035: id da Modalidade Global único quando presente (find-or-create na importação).
        builder.HasIndex(modality => modality.GlobalModalityExternalId)
            .IsUnique()
            .HasFilter("[GlobalModalityExternalId] IS NOT NULL");

        builder.Property(modality => modality.Description)
            .HasMaxLength(1000);

        builder.Property(modality => modality.Status)
            .HasMaxLength(20)
            .IsRequired();

        // Alinhado 1:1 com a migration derivada-da-global-modality (evitar drift de constraint).
        builder.Property(modality => modality.CreatedAt).IsRequired();
        builder.Property(modality => modality.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(modality => modality.UpdatedBy).HasMaxLength(100);
    }
}

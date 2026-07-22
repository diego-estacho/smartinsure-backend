using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInsure.Core.Entities;

namespace SmartInsure.Infra.Data.Mappings;

public sealed class ImportedModalityMapping : IEntityTypeConfiguration<ImportedModality>
{
    public void Configure(EntityTypeBuilder<ImportedModality> builder)
    {
        builder.ToTable("ImportedModalities");

        builder.HasKey(modality => modality.Id);

        builder.Property(modality => modality.InsurerId).IsRequired();
        builder.Property(modality => modality.SourceId).HasMaxLength(100).IsRequired();
        builder.Property(modality => modality.OriginName).HasMaxLength(300).IsRequired();
        builder.Property(modality => modality.Branch).HasMaxLength(20).IsRequired();
        builder.Property(modality => modality.EngineModalityId).HasMaxLength(100);
        builder.Property(modality => modality.EngineModalityName).HasMaxLength(300);
        builder.Property(modality => modality.CommercialParameters);
        builder.Property(modality => modality.Status).HasMaxLength(20).IsRequired();
        builder.Property(modality => modality.IsIgnored).IsRequired();
        builder.Property(modality => modality.LastImportedAt).IsRequired();

        // RN-030: reencontro por (Seguradora, identificador de origem). RN-032: busca por identificador do motor.
        builder.HasIndex(modality => new { modality.InsurerId, modality.SourceId }).IsUnique();
        builder.HasIndex(modality => modality.EngineModalityId);

        builder.HasOne<ImportedGroup>()
            .WithMany()
            .HasForeignKey(modality => modality.ImportedGroupId);

        builder.Property(modality => modality.CreatedAt).IsRequired();
        builder.Property(modality => modality.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(modality => modality.UpdatedBy).HasMaxLength(100);
    }
}

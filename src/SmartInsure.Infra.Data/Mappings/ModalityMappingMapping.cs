using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInsure.Core.Entities;
// Desambigua: no namespace Mappings existe a config `ModalityMapping` (da entidade Modality),
// que sombrearia a entidade `ModalityMapping` (Core). O alias fixa a referência à entidade.
using ModalityMappingEntity = SmartInsure.Core.Entities.ModalityMapping;

namespace SmartInsure.Infra.Data.Mappings;

public sealed class ModalityMappingMapping : IEntityTypeConfiguration<ModalityMappingEntity>
{
    public void Configure(EntityTypeBuilder<ModalityMappingEntity> builder)
    {
        builder.ToTable("ModalityMappings");

        builder.HasKey(mapping => mapping.Id);

        builder.Property(mapping => mapping.ImportedModalityId).IsRequired();
        builder.Property(mapping => mapping.ModalityId).IsRequired();
        builder.Property(mapping => mapping.Establishment).HasMaxLength(20).IsRequired();
        builder.Property(mapping => mapping.Confidence);
        builder.Property(mapping => mapping.Status).HasMaxLength(20).IsRequired();
        builder.Property(mapping => mapping.ConfirmedBy).HasMaxLength(100);
        builder.Property(mapping => mapping.ConfirmedAt);

        builder.HasOne<ImportedModality>()
            .WithMany()
            .HasForeignKey(mapping => mapping.ImportedModalityId);

        builder.HasOne<Modality>()
            .WithMany()
            .HasForeignKey(mapping => mapping.ModalityId);

        builder.HasIndex(mapping => mapping.ImportedModalityId);
        builder.HasIndex(mapping => mapping.ModalityId);

        builder.Property(mapping => mapping.CreatedAt).IsRequired();
        builder.Property(mapping => mapping.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(mapping => mapping.UpdatedBy).HasMaxLength(100);
    }
}

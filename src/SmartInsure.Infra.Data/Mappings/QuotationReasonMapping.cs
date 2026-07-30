using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInsure.Core.Entities;

namespace SmartInsure.Infra.Data.Mappings;

/// <summary>
/// Mapeamento do motivo de indisponibilidade/recusa de uma Cotação (RN-056/RN-058). A relação com a
/// Cotação é configurada em <see cref="QuotationMapping"/> (HasMany Reasons); aqui só os escalares.
/// </summary>
public sealed class QuotationReasonMapping : IEntityTypeConfiguration<QuotationReason>
{
    public void Configure(EntityTypeBuilder<QuotationReason> builder)
    {
        builder.ToTable("QuotationReasons");

        builder.HasKey(reason => reason.Id);
        builder.Property(reason => reason.Id).ValueGeneratedNever();

        builder.Property(reason => reason.QuotationId).IsRequired();
        builder.HasIndex(reason => reason.QuotationId);

        builder.Property(reason => reason.Text).HasMaxLength(500).IsRequired();
        builder.Property(reason => reason.Source).HasMaxLength(20).IsRequired();

        builder.Property(reason => reason.CreatedAt).IsRequired();
        builder.Property(reason => reason.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(reason => reason.UpdatedBy).HasMaxLength(100);
    }
}

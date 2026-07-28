using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInsure.Core.Entities;

namespace SmartInsure.Infra.Data.Mappings;

/// <summary>Mapeamento do motivo de uma Cotação Indisponível/Recusada (RN-058).</summary>
public sealed class QuotationReasonMapping : IEntityTypeConfiguration<QuotationReason>
{
    public void Configure(EntityTypeBuilder<QuotationReason> builder)
    {
        builder.ToTable("QuotationReasons");

        builder.HasKey(reason => reason.Id);
        builder.Property(reason => reason.Id).ValueGeneratedNever();

        builder.Property(reason => reason.QuotationId).IsRequired();

        builder.Property(reason => reason.Text)
            .HasMaxLength(1000)
            .IsRequired();

        builder.HasIndex(reason => reason.QuotationId);

        builder.Property(reason => reason.CreatedAt).IsRequired();
        builder.Property(reason => reason.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(reason => reason.UpdatedBy).HasMaxLength(100);
    }
}

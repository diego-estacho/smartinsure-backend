using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInsure.Core.Entities;

namespace SmartInsure.Infra.Data.Mappings;

/// <summary>Mapeamento de CreditInquiryResultLimit (RN-029..031): limite agrupado por grupo de modalidade.</summary>
public sealed class CreditInquiryResultLimitMapping : IEntityTypeConfiguration<CreditInquiryResultLimit>
{
    public void Configure(EntityTypeBuilder<CreditInquiryResultLimit> builder)
    {
        builder.ToTable("CreditInquiryResultLimits");

        builder.HasKey(limit => limit.Id);

        builder.Property(limit => limit.CreditInquiryResultId).IsRequired();

        builder.Property(limit => limit.GroupName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(limit => limit.GroupType)
            .HasMaxLength(50)
            .IsRequired();

        // Índice único: ResultId + GroupName (um grupo por resultado).
        builder.HasIndex(limit => new { limit.CreditInquiryResultId, limit.GroupName }).IsUnique();

        // Precisão monetária explícita (limites em DECIMAL(18,2), taxas em DECIMAL(9,4)).
        builder.Property(limit => limit.AvailableLimit)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(limit => limit.RevisedLimit)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(limit => limit.Rate)
            .HasPrecision(9, 4)
            .IsRequired();

        // Auditoria (criação é imutável — nunca atualizado).
        builder.Property(limit => limit.CreatedAt).IsRequired();
        builder.Property(limit => limit.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(limit => limit.UpdatedBy).HasMaxLength(100);
    }
}

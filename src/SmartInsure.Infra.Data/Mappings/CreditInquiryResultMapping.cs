using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInsure.Core.Entities;

namespace SmartInsure.Infra.Data.Mappings;

/// <summary>Mapeamento de CreditInquiryResult (RN-029..031): resultado individual de uma consulta por Seguradora.</summary>
public sealed class CreditInquiryResultMapping : IEntityTypeConfiguration<CreditInquiryResult>
{
    public void Configure(EntityTypeBuilder<CreditInquiryResult> builder)
    {
        builder.ToTable("CreditInquiryResults");

        builder.HasKey(result => result.Id);

        builder.Property(result => result.CreditInquiryId).IsRequired();
        builder.Property(result => result.InsurerId).IsRequired();

        // Uma Seguradora aparece uma única vez por consulta (RN-029/RN-030).
        builder.HasIndex(result => new { result.CreditInquiryId, result.InsurerId }).IsUnique();

        builder.Property(result => result.Status)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(result => result.FailureReason)
            .HasMaxLength(500);

        // Precisão monetária explícita (limites em DECIMAL(18,2), taxas em DECIMAL(9,4)).
        builder.Property(result => result.TraditionalLimit)
            .HasPrecision(18, 2);
        builder.Property(result => result.TraditionalRate)
            .HasPrecision(9, 4);

        builder.Property(result => result.JudicialLimit)
            .HasPrecision(18, 2);
        builder.Property(result => result.JudicialRate)
            .HasPrecision(9, 4);
        builder.Property(result => result.JudicialFiscalRate)
            .HasPrecision(9, 4);

        builder.Property(result => result.FinancialLimit)
            .HasPrecision(18, 2);
        builder.Property(result => result.FinancialRate)
            .HasPrecision(9, 4);

        builder.Property(result => result.LimitValidUntil);

        // Auditoria (criação é imutável — nunca atualizado).
        builder.Property(result => result.CreatedAt).IsRequired();
        builder.Property(result => result.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(result => result.UpdatedBy).HasMaxLength(100);
    }
}

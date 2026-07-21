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

        // FK com DeleteBehavior.Restrict (convenção global, ADR-034).
        builder.HasOne<Insurer>()
            .WithMany()
            .HasForeignKey(result => result.InsurerId);

        // Índice para queries por Seguradora (RN-031).
        builder.HasIndex(result => result.InsurerId);

        builder.Property(result => result.Status)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(result => result.FailureReason)
            .HasMaxLength(500);

        // Coleção filha de limites — mapeamento com acesso por field (RN-029).
        builder.HasMany(result => result.Limits)
            .WithOne()
            .HasForeignKey(limit => limit.CreditInquiryResultId)
            .IsRequired();

        builder.Navigation(result => result.Limits)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // Auditoria (criação é imutável — nunca atualizado).
        builder.Property(result => result.CreatedAt).IsRequired();
        builder.Property(result => result.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(result => result.UpdatedBy).HasMaxLength(100);
    }
}

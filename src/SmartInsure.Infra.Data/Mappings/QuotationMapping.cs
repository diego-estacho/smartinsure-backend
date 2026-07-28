using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInsure.Core.Entities;

namespace SmartInsure.Infra.Data.Mappings;

/// <summary>Mapeamento da Cotação (RN-057..061): o retorno de uma Seguradora para um Grupo de Cotação.</summary>
public sealed class QuotationMapping : IEntityTypeConfiguration<Quotation>
{
    public void Configure(EntityTypeBuilder<Quotation> builder)
    {
        builder.ToTable("Quotations");

        builder.HasKey(quotation => quotation.Id);
        builder.Property(quotation => quotation.Id).ValueGeneratedNever();

        builder.Property(quotation => quotation.QuotationGroupId).IsRequired();
        builder.Property(quotation => quotation.InsurerId).IsRequired();

        // Uma Cotação por (Grupo, Seguradora) — o fan-out não duplica (RN-057).
        builder.HasIndex(quotation => new { quotation.QuotationGroupId, quotation.InsurerId }).IsUnique();

        // FKs com DeleteBehavior.Restrict (convenção global, ADR-034).
        builder.HasOne<QuotationGroup>()
            .WithMany()
            .HasForeignKey(quotation => quotation.QuotationGroupId);

        builder.HasOne<Insurer>()
            .WithMany()
            .HasForeignKey(quotation => quotation.InsurerId);

        builder.Property(quotation => quotation.ProcessingStatus)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(quotation => quotation.Result)
            .HasMaxLength(20);

        builder.Property(quotation => quotation.AnalysisTrack)
            .HasMaxLength(20);

        builder.Property(quotation => quotation.Premium).HasPrecision(18, 2);
        builder.Property(quotation => quotation.CommissionPercentage).HasPrecision(9, 4);
        builder.Property(quotation => quotation.CommissionValue).HasPrecision(18, 2);
        builder.Property(quotation => quotation.Tax).HasPrecision(18, 2);
        builder.Property(quotation => quotation.AvailableLimit).HasPrecision(18, 2);
        builder.Property(quotation => quotation.CcgMaxLimitWithoutNeed).HasPrecision(18, 2);

        builder.Property(quotation => quotation.ProposalExternalId).HasMaxLength(100);
        builder.Property(quotation => quotation.ProposalNumber).HasMaxLength(50);

        builder.Property(quotation => quotation.RequiresCcg).IsRequired();
        builder.Property(quotation => quotation.CcgSigned).IsRequired();

        // Índice do reconciliador (ADR-050): varredura das Requested por antiguidade.
        builder.HasIndex(quotation => new { quotation.ProcessingStatus, quotation.CreatedAt });

        // Coleção filha de motivos — acesso por field (RN-058).
        builder.HasMany(quotation => quotation.Reasons)
            .WithOne()
            .HasForeignKey(reason => reason.QuotationId)
            .IsRequired();

        builder.Navigation(quotation => quotation.Reasons)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(quotation => quotation.CreatedAt).IsRequired();
        builder.Property(quotation => quotation.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(quotation => quotation.UpdatedBy).HasMaxLength(100);
    }
}

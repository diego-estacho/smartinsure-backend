using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInsure.Core.Entities;

namespace SmartInsure.Infra.Data.Mappings;

/// <summary>
/// Mapeamento da Cotação (RN-057/RN-058): resultado por Seguradora dentro de um Grupo de Cotação.
/// Alinhado 1:1 com a migration criar-tabelas-cotacoes (evitar drift de constraint).
/// </summary>
public sealed class QuotationMapping : IEntityTypeConfiguration<Quotation>
{
    public void Configure(EntityTypeBuilder<Quotation> builder)
    {
        builder.ToTable("Quotations");

        builder.HasKey(quotation => quotation.Id);
        builder.Property(quotation => quotation.Id).ValueGeneratedNever();

        // FKs navigation-less, Restrict pela convenção global (ADR-034).
        builder.HasOne<QuotationGroup>()
            .WithMany()
            .HasForeignKey(quotation => quotation.QuotationGroupId);

        builder.HasOne<Insurer>()
            .WithMany()
            .HasForeignKey(quotation => quotation.InsurerId);

        // Uma Cotação por Seguradora dentro de um Grupo (idempotência do fan-out — RN-057).
        builder.HasIndex(quotation => new { quotation.QuotationGroupId, quotation.InsurerId }).IsUnique();
        builder.HasIndex(quotation => quotation.InsurerId);

        builder.Property(quotation => quotation.ProcessingStatus)
            .HasMaxLength(20)
            .IsRequired();

        // Enums NULLABLE: a convenção global (Properties<Enum>) cobre só enum não-nullable — conversão explícita aqui.
        builder.Property(quotation => quotation.Result)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(quotation => quotation.AnalysisTrack)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(quotation => quotation.Premium).HasPrecision(18, 2);
        builder.Property(quotation => quotation.CommissionPercentage).HasPrecision(9, 4);
        builder.Property(quotation => quotation.CommissionValue).HasPrecision(18, 2);
        builder.Property(quotation => quotation.Tax).HasPrecision(9, 4);
        builder.Property(quotation => quotation.AvailableLimit).HasPrecision(18, 2);
        builder.Property(quotation => quotation.CcgMaxLimitWithoutNeed).HasPrecision(18, 2);

        builder.Property(quotation => quotation.ProposalExternalId).HasMaxLength(100);
        builder.Property(quotation => quotation.ProposalNumber).HasMaxLength(50);

        builder.Property(quotation => quotation.RequiresCcg).IsRequired();
        builder.Property(quotation => quotation.CcgSigned).IsRequired();

        // Lease do fan-out (ADR-050): instante em que o consumidor começou a processar; usado pelo reconciliador.
        builder.Property(quotation => quotation.ProcessingStartedAt);

        // Índice filtrado do reconciliador (só as em voo): varredura barata a cada tick, sem tocar o histórico.
        builder.HasIndex(quotation => new { quotation.ProcessingStartedAt, quotation.CreatedAt })
            .HasDatabaseName("IX_Quotations_Requested")
            .HasFilter("[ProcessingStatus] = 'Requested'");

        // Coleção filha de motivos — acesso por field (backing list _reasons). A FK nasce Restrict pela
        // convenção global (ADR-034), que sobrescreve qualquer OnDelete daqui; por isso a remoção em
        // cascata do agregado (recálculo RN-060) é feita explicitamente no QuotationRepository.Remove
        // (apaga os motivos carregados antes da Cotação).
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

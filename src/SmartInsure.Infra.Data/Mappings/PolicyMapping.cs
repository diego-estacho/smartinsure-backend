using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInsure.Core.Entities;

namespace SmartInsure.Infra.Data.Mappings;

/// <summary>
/// Mapeamento da Apólice (RN-514): uma por Cotação (índice único — é o que garante a solicitação única
/// da RN-507 no banco, não só na regra). Guarda os valores emitidos e o snapshot do endereço enviado.
/// </summary>
public sealed class PolicyMapping : IEntityTypeConfiguration<Policy>
{
    public void Configure(EntityTypeBuilder<Policy> builder)
    {
        builder.ToTable("Policies");

        builder.HasKey(policy => policy.Id);

        builder.Property(policy => policy.Id).ValueGeneratedNever();

        builder.HasOne<QuotationGroup>()
            .WithMany()
            .HasForeignKey(policy => policy.QuotationGroupId);

        builder.HasOne<Quotation>()
            .WithMany()
            .HasForeignKey(policy => policy.QuotationId);

        builder.HasOne<TermAcceptance>()
            .WithMany()
            .HasForeignKey(policy => policy.TermAcceptanceId);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(policy => policy.RequestedByUserId);

        builder.Property(policy => policy.PolicyExternalId).HasMaxLength(100).IsRequired();
        builder.Property(policy => policy.ProposalNumber).HasMaxLength(50);

        builder.Property(policy => policy.Premium).HasPrecision(18, 2);
        builder.Property(policy => policy.Tax).HasPrecision(9, 4);
        builder.Property(policy => policy.CommissionPercentage).HasPrecision(9, 4);
        builder.Property(policy => policy.CommissionValue).HasPrecision(18, 2);

        builder.Property(policy => policy.InstallmentNumber).IsRequired();
        builder.Property(policy => policy.GracePeriodInDays).IsRequired();
        builder.Property(policy => policy.RequestedAt).IsRequired();

        builder.Property(policy => policy.InsuredAddressZipCode).HasMaxLength(8);
        builder.Property(policy => policy.InsuredAddressStreet).HasMaxLength(200);
        builder.Property(policy => policy.InsuredAddressNumber).HasMaxLength(20);
        builder.Property(policy => policy.InsuredAddressComplement).HasMaxLength(100);
        builder.Property(policy => policy.InsuredAddressNeighborhood).HasMaxLength(100);
        builder.Property(policy => policy.InsuredAddressCity).HasMaxLength(100);
        builder.Property(policy => policy.InsuredAddressState).HasMaxLength(2);

        // RN-507: uma única Apólice por Cotação.
        builder.HasIndex(policy => policy.QuotationId).IsUnique();
        builder.HasIndex(policy => policy.QuotationGroupId);

        builder.Property(policy => policy.CreatedAt).IsRequired();
        builder.Property(policy => policy.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(policy => policy.UpdatedBy).HasMaxLength(100);
    }
}

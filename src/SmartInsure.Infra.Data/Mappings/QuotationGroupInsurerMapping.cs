using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInsure.Core.Entities;

namespace SmartInsure.Infra.Data.Mappings;

/// <summary>Mapeamento da Seguradora do escopo de um Grupo de Cotação (RN-050).</summary>
public sealed class QuotationGroupInsurerMapping : IEntityTypeConfiguration<QuotationGroupInsurer>
{
    public void Configure(EntityTypeBuilder<QuotationGroupInsurer> builder)
    {
        builder.ToTable("QuotationGroupInsurers");

        builder.HasKey(insurer => insurer.Id);

        builder.Property(insurer => insurer.QuotationGroupId).IsRequired();
        builder.Property(insurer => insurer.InsurerId).IsRequired();

        // Uma Seguradora aparece uma única vez por Grupo de Cotação.
        builder.HasIndex(insurer => new { insurer.QuotationGroupId, insurer.InsurerId }).IsUnique();

        // FK com DeleteBehavior.Restrict (convenção global, ADR-034).
        builder.HasOne<Insurer>()
            .WithMany()
            .HasForeignKey(insurer => insurer.InsurerId);

        builder.Property(insurer => insurer.CreatedAt).IsRequired();
        builder.Property(insurer => insurer.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(insurer => insurer.UpdatedBy).HasMaxLength(100);
    }
}

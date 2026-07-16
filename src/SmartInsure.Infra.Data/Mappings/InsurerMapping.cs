using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInsure.Core.Entities;

namespace SmartInsure.Infra.Data.Mappings;

public sealed class InsurerMapping : IEntityTypeConfiguration<Insurer>
{
    public void Configure(EntityTypeBuilder<Insurer> builder)
    {
        builder.ToTable("Insurers");

        builder.HasKey(insurer => insurer.Id);

        builder.Property(insurer => insurer.Cnpj)
            .HasMaxLength(14)
            .IsRequired();

        // RN-005/RN-006: CNPJ único no catálogo.
        builder.HasIndex(insurer => insurer.Cnpj).IsUnique();

        builder.Property(insurer => insurer.CorporateName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(insurer => insurer.TradeName)
            .HasMaxLength(200);

        builder.Property(insurer => insurer.LogoUrl)
            .HasMaxLength(500);

        builder.Property(insurer => insurer.Status)
            .HasMaxLength(20)
            .IsRequired();

        // Alinhado 1:1 com a migration criar-tabela-insurers (evitar drift de constraint).
        builder.Property(insurer => insurer.CreatedAt).IsRequired();
        builder.Property(insurer => insurer.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(insurer => insurer.UpdatedBy).HasMaxLength(100);
    }
}

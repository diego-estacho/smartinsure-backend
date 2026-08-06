using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInsure.Core.Entities;

namespace SmartInsure.Infra.Data.Mappings;

/// <summary>
/// Mapeamento do Endereço do Segurado da oferta (RN-503): réplica 1:1 com o Grupo de Cotação. Tamanhos
/// espelham PersonAddress — é uma cópia dos mesmos campos, não um endereço de outra natureza.
/// </summary>
public sealed class QuotationAddressMapping : IEntityTypeConfiguration<QuotationAddress>
{
    public void Configure(EntityTypeBuilder<QuotationAddress> builder)
    {
        builder.ToTable("QuotationAddresses");

        builder.HasKey(address => address.Id);

        builder.Property(address => address.Id).ValueGeneratedNever();

        builder.Property(address => address.ZipCode).HasMaxLength(8);
        builder.Property(address => address.Street).HasMaxLength(200);
        builder.Property(address => address.Number).HasMaxLength(20);
        builder.Property(address => address.Complement).HasMaxLength(100);
        builder.Property(address => address.Neighborhood).HasMaxLength(100);
        builder.Property(address => address.City).HasMaxLength(100);
        builder.Property(address => address.State).HasMaxLength(2);

        // Uma réplica por oferta: índice único no Grupo (o relacionamento é declarado no lado do Grupo).
        builder.HasIndex(address => address.QuotationGroupId).IsUnique();

        builder.Property(address => address.CreatedAt).IsRequired();
        builder.Property(address => address.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(address => address.UpdatedBy).HasMaxLength(100);
    }
}

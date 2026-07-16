using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInsure.Core.Entities;

namespace SmartInsure.Infra.Data.Mappings;

public sealed class PersonAddressMapping : IEntityTypeConfiguration<PersonAddress>
{
    public void Configure(EntityTypeBuilder<PersonAddress> builder)
    {
        builder.ToTable("PersonAddresses");

        builder.HasKey(address => address.Id);

        builder.HasIndex(address => address.PersonId);

        builder.Property(address => address.ZipCode).HasMaxLength(8);
        builder.Property(address => address.Street).HasMaxLength(200);
        builder.Property(address => address.Number).HasMaxLength(20);
        builder.Property(address => address.Complement).HasMaxLength(200);
        builder.Property(address => address.Neighborhood).HasMaxLength(100);
        builder.Property(address => address.City).HasMaxLength(100);
        builder.Property(address => address.State).HasMaxLength(2);
        builder.Property(address => address.IsMain).IsRequired();

        // Alinhado 1:1 com a migration criar-tabelas-persons (evitar drift de constraint).
        builder.Property(address => address.CreatedAt).IsRequired();
        builder.Property(address => address.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(address => address.UpdatedBy).HasMaxLength(100);
    }
}

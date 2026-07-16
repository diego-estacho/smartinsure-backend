using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInsure.Core.Entities;

namespace SmartInsure.Infra.Data.Mappings;

public sealed class PersonMapping : IEntityTypeConfiguration<Person>
{
    public void Configure(EntityTypeBuilder<Person> builder)
    {
        builder.ToTable("Persons");

        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Cnpj)
            .HasMaxLength(14)
            .IsRequired();

        // RN-013/RN-014: uma Pessoa por CNPJ.
        builder.HasIndex(entity => entity.Cnpj).IsUnique();

        builder.Property(entity => entity.CorporateName)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(entity => entity.CorporateName);

        builder.Property(entity => entity.TradeName)
            .HasMaxLength(200);

        builder.HasOne(entity => entity.LegalNature)
            .WithMany()
            .HasForeignKey(entity => entity.LegalNatureId)
            .IsRequired();

        builder.HasMany(entity => entity.Addresses)
            .WithOne()
            .HasForeignKey(address => address.PersonId)
            .IsRequired();

        builder.Navigation(entity => entity.Addresses)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // Alinhado 1:1 com a migration criar-tabelas-persons (evitar drift de constraint).
        builder.Property(entity => entity.CreatedAt).IsRequired();
        builder.Property(entity => entity.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(entity => entity.UpdatedBy).HasMaxLength(100);
    }
}

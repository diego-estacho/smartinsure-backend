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

        builder.Property(entity => entity.DocumentNumber)
            .HasMaxLength(14)
            .IsRequired();

        // RN-013/RN-014: uma Pessoa por documento (CPF/CNPJ).
        builder.HasIndex(entity => entity.DocumentNumber).IsUnique();

        builder.Property(entity => entity.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(entity => entity.Name);

        builder.Property(entity => entity.SocialName)
            .HasMaxLength(200);

        builder.Property(entity => entity.Type)
            .HasMaxLength(1)
            .IsRequired();

        // RN-015: Natureza Jurídica só existe para pessoa jurídica.
        builder.HasOne(entity => entity.LegalNature)
            .WithMany()
            .HasForeignKey(entity => entity.LegalNatureId);

        builder.HasMany(entity => entity.Addresses)
            .WithOne()
            .HasForeignKey(address => address.PersonId)
            .IsRequired();

        builder.Navigation(entity => entity.Addresses)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(entity => entity.Roles)
            .WithOne()
            .HasForeignKey(role => role.PersonId)
            .IsRequired();

        builder.Navigation(entity => entity.Roles)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // Alinhado 1:1 com a migration criar-tabelas-persons (evitar drift de constraint).
        builder.Property(entity => entity.CreatedAt).IsRequired();
        builder.Property(entity => entity.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(entity => entity.UpdatedBy).HasMaxLength(100);
    }
}

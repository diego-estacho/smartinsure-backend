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

        // RN-101/ADR-101: Filial vinculada à matriz (self-FK), coluna e FK_Persons_Headquarters
        // criadas pela migration Flyway. Um Property escalar aqui não basta — sem HasOne o EF
        // não infere relação nenhuma de um Guid? "nu" sem navegação, então nada garante a ordem
        // de gravação entre matriz e Filial num mesmo SaveChanges; espelha QuotationGroupMapping
        // (BranchPersonId), mesmo padrão de self-FK opcional sem navegação.
        builder.HasOne<Person>()
            .WithMany()
            .HasForeignKey(entity => entity.HeadquartersPersonId)
            .OnDelete(DeleteBehavior.NoAction);

        // Filtrado: espelha a migration Flyway (só filiais têm HeadquartersPersonId preenchido).
        builder.HasIndex(entity => entity.HeadquartersPersonId)
            .HasDatabaseName("IX_Persons_HeadquartersPersonId")
            .HasFilter("[HeadquartersPersonId] IS NOT NULL");

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

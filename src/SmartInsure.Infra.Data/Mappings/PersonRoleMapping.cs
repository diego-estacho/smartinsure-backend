using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInsure.Core.Entities;

namespace SmartInsure.Infra.Data.Mappings;

public sealed class PersonRoleMapping : IEntityTypeConfiguration<PersonRole>
{
    public void Configure(EntityTypeBuilder<PersonRole> builder)
    {
        builder.ToTable("PersonRoles");

        builder.HasKey(role => role.Id);

        builder.Property(role => role.Role)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(role => role.Status)
            .HasMaxLength(20)
            .IsRequired();

        // RN-034: contato complementar da Corretora (só o papel Corretor usa; nulo nos demais).
        // Alinhado 1:1 com a migration adicionar-contato-corretor-em-person-roles.
        builder.Property(role => role.ContactEmail).HasMaxLength(200);
        builder.Property(role => role.ContactPhone).HasMaxLength(20);
        builder.Property(role => role.ResponsibleName).HasMaxLength(200);

        // RN-017: um vínculo por papel por Pessoa.
        builder.HasIndex(role => new { role.PersonId, role.Role })
            .HasDatabaseName("UX_PersonRoles_PersonId_Role")
            .IsUnique();

        // Alinhado 1:1 com a migration criar-tabela-person-roles (evitar drift de constraint).
        builder.Property(role => role.CreatedAt).IsRequired();
        builder.Property(role => role.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(role => role.UpdatedBy).HasMaxLength(100);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInsure.Core.Entities;

namespace SmartInsure.Infra.Data.Mappings;

public sealed class UserMapping : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(user => user.Id);

        builder.Property(user => user.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(user => user.Email)
            .HasMaxLength(320)
            .IsRequired();

        // RN-082: CPF (somente dígitos), único por índice filtrado — pré-RN-082 fica NULL (não colide).
        builder.Property(user => user.DocumentNumber)
            .HasMaxLength(11);

        // RN-001: e-mail único na plataforma e identidade única no provedor.
        builder.HasIndex(user => user.Email).IsUnique();
        builder.HasIndex(user => user.ExternalIdentity).IsUnique();
        builder.HasIndex(user => user.DocumentNumber)
            .IsUnique()
            .HasFilter("[DocumentNumber] IS NOT NULL");

        builder.Property(user => user.ExternalIdentity)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(user => user.Status)
            .HasMaxLength(20)
            .IsRequired();

        // RN-204: último acesso (login concluído). Nulo = nunca acessou. Migration V20260808120000.
        builder.Property(user => user.LastAccessAtUtc);

        // RN-012/RN-062: Perfil (Escopo System) opcional, referenciado por FK (nullable).
        builder.Property(user => user.ProfileId);
        builder.HasOne(user => user.Profile)
            .WithMany()
            .HasForeignKey(user => user.ProfileId);

        // Alinhado 1:1 com a migration V20260715114410 (evitar drift de constraint).
        builder.Property(user => user.CreatedAt).IsRequired();
        builder.Property(user => user.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(user => user.UpdatedBy).HasMaxLength(100);
    }
}

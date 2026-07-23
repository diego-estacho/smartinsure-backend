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

        // RN-001: e-mail único na plataforma e identidade única no provedor.
        builder.HasIndex(user => user.Email).IsUnique();
        builder.HasIndex(user => user.ExternalIdentity).IsUnique();

        builder.Property(user => user.ExternalIdentity)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(user => user.Status)
            .HasMaxLength(20)
            .IsRequired();

        // RN-012/RN-032: Perfil (Escopo System) opcional, referenciado por FK (nullable).
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

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

        builder.Property(user => user.CreatedBy).HasMaxLength(100);
        builder.Property(user => user.UpdatedBy).HasMaxLength(100);
    }
}

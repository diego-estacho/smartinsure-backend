using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInsure.Core.Entities;

namespace SmartInsure.Infra.Data.Mappings;

public sealed class ProfileMapping : IEntityTypeConfiguration<Profile>
{
    public void Configure(EntityTypeBuilder<Profile> builder)
    {
        builder.ToTable("Profiles");

        builder.HasKey(profile => profile.Id);

        builder.Property(profile => profile.Name)
            .HasMaxLength(100)
            .IsRequired();

        // RN-032: nome de Perfil único (nesta fatia só há Escopo System).
        builder.HasIndex(profile => profile.Name).IsUnique();

        builder.Property(profile => profile.Scope)
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(profile => profile.Scope);

        builder.Property(profile => profile.IsFixed).IsRequired();

        builder.Property(profile => profile.BrokerageId);
        builder.Property(profile => profile.PolicyHolderId);

        // Coleção privada de Permissões do Perfil (RN-032/RN-033).
        builder.HasMany(profile => profile.Permissions)
            .WithOne()
            .HasForeignKey(profilePermission => profilePermission.ProfileId)
            .IsRequired();

        builder.Navigation(profile => profile.Permissions)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // Alinhado 1:1 com a migration V20260723144712 (evitar drift de constraint).
        builder.Property(profile => profile.CreatedAt).IsRequired();
        builder.Property(profile => profile.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(profile => profile.UpdatedBy).HasMaxLength(100);
    }
}

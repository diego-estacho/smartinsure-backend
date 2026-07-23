using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInsure.Core.Entities;

namespace SmartInsure.Infra.Data.Mappings;

public sealed class ProfilePermissionMapping : IEntityTypeConfiguration<ProfilePermission>
{
    public void Configure(EntityTypeBuilder<ProfilePermission> builder)
    {
        builder.ToTable("ProfilePermissions");

        builder.HasKey(profilePermission => profilePermission.Id);

        builder.Property(profilePermission => profilePermission.ProfileId).IsRequired();
        builder.Property(profilePermission => profilePermission.PermissionId).IsRequired();

        // RN-032/RN-033: par Perfil×Permissão único.
        builder.HasIndex(profilePermission => new { profilePermission.ProfileId, profilePermission.PermissionId })
            .IsUnique();

        builder.HasOne<Permission>()
            .WithMany()
            .HasForeignKey(profilePermission => profilePermission.PermissionId)
            .IsRequired();

        // Alinhado 1:1 com a migration V20260723144719 (evitar drift de constraint).
        builder.Property(profilePermission => profilePermission.CreatedAt).IsRequired();
        builder.Property(profilePermission => profilePermission.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(profilePermission => profilePermission.UpdatedBy).HasMaxLength(100);
    }
}

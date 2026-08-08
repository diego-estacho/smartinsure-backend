using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInsure.Core.Entities;

namespace SmartInsure.Infra.Data.Mappings;

public sealed class PermissionMapping : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("Permissions");

        builder.HasKey(permission => permission.Id);

        builder.Property(permission => permission.Code)
            .HasMaxLength(100)
            .IsRequired();

        // RN-063: código único no catálogo.
        builder.HasIndex(permission => permission.Code).IsUnique();

        builder.Property(permission => permission.Description)
            .HasMaxLength(500);

        builder.Property(permission => permission.IsSystem).IsRequired();

        // RN-063 (revisão 2026-08-07): Área e dependência do catálogo — alinhado 1:1 com a migration
        // V20260807100000 (colunas nullable). O rótulo pt-BR da Área e a nota explicativa são copy de
        // UI e moram no front (ADR-058).
        builder.Property(permission => permission.Area).HasMaxLength(50);
        builder.Property(permission => permission.DependsOn).HasMaxLength(100);

        // Alinhado 1:1 com a migration V20260723144704 (evitar drift de constraint).
        builder.Property(permission => permission.CreatedAt).IsRequired();
        builder.Property(permission => permission.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(permission => permission.UpdatedBy).HasMaxLength(100);
    }
}

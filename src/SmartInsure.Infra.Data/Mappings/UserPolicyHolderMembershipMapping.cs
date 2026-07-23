using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInsure.Core.Entities;

namespace SmartInsure.Infra.Data.Mappings;

public sealed class UserPolicyHolderMembershipMapping : IEntityTypeConfiguration<UserPolicyHolderMembership>
{
    public void Configure(EntityTypeBuilder<UserPolicyHolderMembership> builder)
    {
        builder.ToTable("UserPolicyHolderMemberships");

        builder.HasKey(membership => membership.Id);

        builder.Property(membership => membership.UserId).IsRequired();
        builder.Property(membership => membership.PolicyHolderId).IsRequired();
        builder.Property(membership => membership.ProfileId).IsRequired();

        // RN-034: no máximo um vínculo por Usuário × Tomador.
        builder.HasIndex(membership => new { membership.UserId, membership.PolicyHolderId }).IsUnique();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(membership => membership.UserId)
            .IsRequired();

        // Tomador é uma Person (papel PolicyHolder) — mesmo padrão do PolicyHolderAppointment.
        builder.HasOne<Person>()
            .WithMany()
            .HasForeignKey(membership => membership.PolicyHolderId)
            .IsRequired();

        builder.HasOne<Profile>()
            .WithMany()
            .HasForeignKey(membership => membership.ProfileId)
            .IsRequired();

        // Alinhado 1:1 com a migration V20260723154504 (evitar drift de constraint).
        builder.Property(membership => membership.CreatedAt).IsRequired();
        builder.Property(membership => membership.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(membership => membership.UpdatedBy).HasMaxLength(100);
    }
}

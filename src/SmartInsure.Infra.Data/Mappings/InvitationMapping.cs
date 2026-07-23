using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInsure.Core.Entities;

namespace SmartInsure.Infra.Data.Mappings;

public sealed class InvitationMapping : IEntityTypeConfiguration<Invitation>
{
    public void Configure(EntityTypeBuilder<Invitation> builder)
    {
        builder.ToTable("Invitations");

        builder.HasKey(invitation => invitation.Id);

        builder.Property(invitation => invitation.UserId).IsRequired();
        builder.Property(invitation => invitation.TokenHash).HasMaxLength(64).IsRequired();
        builder.Property(invitation => invitation.ExpiresAtUtc).IsRequired();
        builder.Property(invitation => invitation.ConsumedAtUtc);

        // RN-035: no máximo um convite ativo (ConsumedAtUtc IS NULL) por Usuário.
        builder.HasIndex(invitation => invitation.UserId)
            .IsUnique()
            .HasFilter("[ConsumedAtUtc] IS NULL");

        builder.HasIndex(invitation => invitation.TokenHash);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(invitation => invitation.UserId)
            .IsRequired();

        // Alinhado 1:1 com a migration V20260723164510 (evitar drift de constraint).
        builder.Property(invitation => invitation.CreatedAt).IsRequired();
        builder.Property(invitation => invitation.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(invitation => invitation.UpdatedBy).HasMaxLength(100);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInsure.Core.Entities;

namespace SmartInsure.Infra.Data.Mappings;

public sealed class UserBrokerageMembershipMapping : IEntityTypeConfiguration<UserBrokerageMembership>
{
    public void Configure(EntityTypeBuilder<UserBrokerageMembership> builder)
    {
        builder.ToTable("UserBrokerageMemberships");

        builder.HasKey(membership => membership.Id);

        builder.Property(membership => membership.UserId).IsRequired();
        builder.Property(membership => membership.BrokerageId).IsRequired();
        builder.Property(membership => membership.ProfileId).IsRequired();

        // RN-034: no máximo um vínculo por Usuário × Corretora.
        builder.HasIndex(membership => new { membership.UserId, membership.BrokerageId }).IsUnique();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(membership => membership.UserId)
            .IsRequired();

        // Corretora é uma Person (papel Broker) — mesmo padrão do PolicyHolderAppointment.
        builder.HasOne<Person>()
            .WithMany()
            .HasForeignKey(membership => membership.BrokerageId)
            .IsRequired();

        builder.HasOne<Profile>()
            .WithMany()
            .HasForeignKey(membership => membership.ProfileId)
            .IsRequired();

        // Alinhado 1:1 com a migration V20260723154456 (evitar drift de constraint).
        builder.Property(membership => membership.CreatedAt).IsRequired();
        builder.Property(membership => membership.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(membership => membership.UpdatedBy).HasMaxLength(100);
    }
}

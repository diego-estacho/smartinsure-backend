using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInsure.Core.Entities;

namespace SmartInsure.Infra.Data.Mappings;

public sealed class PolicyHolderAppointmentMapping : IEntityTypeConfiguration<PolicyHolderAppointment>
{
    public void Configure(EntityTypeBuilder<PolicyHolderAppointment> builder)
    {
        builder.ToTable("PolicyHolderAppointments");

        builder.HasKey(appointment => appointment.Id);

        // RN-027: máximo uma Nomeação Vigente por par Tomador×Seguradora.
        builder.HasIndex(appointment => new { appointment.PolicyHolderId, appointment.InsurerId })
            .IsUnique()
            .HasFilter("[Status] = 'Active'");

        builder.HasOne<Person>()
            .WithMany()
            .HasForeignKey(appointment => appointment.PolicyHolderId);

        builder.HasOne<Person>()
            .WithMany()
            .HasForeignKey(appointment => appointment.BrokerageId);

        builder.HasOne<Insurer>()
            .WithMany()
            .HasForeignKey(appointment => appointment.InsurerId);

        builder.Property(appointment => appointment.Status)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(appointment => appointment.StartedAt)
            .IsRequired();

        builder.Property(appointment => appointment.EndedAt);

        // Alinhado 1:1 com a migration criar-tabela-policy-holder-appointments (evitar drift de constraint).
        builder.Property(appointment => appointment.CreatedAt).IsRequired();
        builder.Property(appointment => appointment.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(appointment => appointment.UpdatedBy).HasMaxLength(100);
    }
}

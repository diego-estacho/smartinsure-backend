using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInsure.Core.Entities;

namespace SmartInsure.Infra.Data.Mappings;

public sealed class InsurerEnablementMapping : IEntityTypeConfiguration<InsurerEnablement>
{
    public void Configure(EntityTypeBuilder<InsurerEnablement> builder)
    {
        builder.ToTable("InsurerEnablements");

        builder.HasKey(enablement => enablement.Id);

        // RN-022: no máximo uma Habilitação por par Corretora×Seguradora.
        builder.HasIndex(enablement => new { enablement.BrokerageId, enablement.InsurerId }).IsUnique();

        builder.HasOne<Person>()
            .WithMany()
            .HasForeignKey(enablement => enablement.BrokerageId);

        builder.HasOne<Insurer>()
            .WithMany()
            .HasForeignKey(enablement => enablement.InsurerId);

        builder.Property(enablement => enablement.CalculationEngine)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(enablement => enablement.ConnectionParameters);

        builder.Property(enablement => enablement.Status)
            .HasMaxLength(20)
            .IsRequired();

        // Alinhado 1:1 com a migration criar-tabela-insurer-enablements (evitar drift de constraint).
        builder.Property(enablement => enablement.CreatedAt).IsRequired();
        builder.Property(enablement => enablement.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(enablement => enablement.UpdatedBy).HasMaxLength(100);
    }
}

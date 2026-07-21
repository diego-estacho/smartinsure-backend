using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInsure.Core.Entities;

namespace SmartInsure.Infra.Data.Mappings;

/// <summary>Mapeamento de CreditInquiry (RN-029..031): consulta de limites de crédito do tomador.</summary>
public sealed class CreditInquiryMapping : IEntityTypeConfiguration<CreditInquiry>
{
    public void Configure(EntityTypeBuilder<CreditInquiry> builder)
    {
        builder.ToTable("CreditInquiries");

        builder.HasKey(inquiry => inquiry.Id);

        builder.Property(inquiry => inquiry.BrokerageId).IsRequired();
        builder.Property(inquiry => inquiry.PolicyHolderCnpj)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(inquiry => inquiry.PolicyHolderName)
            .HasMaxLength(200);

        builder.Property(inquiry => inquiry.QueriedAt).IsRequired();

        // RN-031: histórico consultável por CNPJ e por Corretora.
        builder.HasIndex(inquiry => inquiry.PolicyHolderCnpj);
        builder.HasIndex(inquiry => inquiry.BrokerageId);

        // FK com DeleteBehavior.Restrict (convenção global, ADR-034).
        builder.HasOne<Person>()
            .WithMany()
            .HasForeignKey(inquiry => inquiry.BrokerageId);

        // Coleção filha de resultados — mapeamento com acesso por field.
        builder.HasMany(inquiry => inquiry.Results)
            .WithOne()
            .HasForeignKey(result => result.CreditInquiryId)
            .IsRequired();

        builder.Navigation(inquiry => inquiry.Results)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // Auditoria (criação é imutável — nunca atualizado).
        builder.Property(inquiry => inquiry.CreatedAt).IsRequired();
        builder.Property(inquiry => inquiry.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(inquiry => inquiry.UpdatedBy).HasMaxLength(100);
    }
}

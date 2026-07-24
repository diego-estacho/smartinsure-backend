using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInsure.Core.Entities;

namespace SmartInsure.Infra.Data.Mappings;

/// <summary>Mapeamento do Grupo de Cotação (RN-050/RN-051): o pedido do corretor persistido em Rascunho.</summary>
public sealed class QuotationGroupMapping : IEntityTypeConfiguration<QuotationGroup>
{
    public void Configure(EntityTypeBuilder<QuotationGroup> builder)
    {
        builder.ToTable("QuotationGroups");

        builder.HasKey(group => group.Id);

        // FKs com DeleteBehavior.Restrict (convenção global, ADR-034). Tomador e Segurado são Pessoas.
        builder.HasOne<Person>()
            .WithMany()
            .HasForeignKey(group => group.PolicyHolderId);

        builder.HasOne<Person>()
            .WithMany()
            .HasForeignKey(group => group.InsuredId);

        builder.HasOne<Modality>()
            .WithMany()
            .HasForeignKey(group => group.ModalityId);

        builder.Property(group => group.InsuredAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(group => group.CoverageStartDate).IsRequired();
        builder.Property(group => group.CoverageEndDate).IsRequired();

        builder.Property(group => group.ScopeMode)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(group => group.IncludesPenaltyCoverage).IsRequired();
        builder.Property(group => group.IncludesLaborCoverage).IsRequired();

        builder.Property(group => group.Status)
            .HasMaxLength(20)
            .IsRequired();

        // Histórico consultável por tomador e por segurado.
        builder.HasIndex(group => group.PolicyHolderId);
        builder.HasIndex(group => group.InsuredId);

        // Coleção filha das Seguradoras do escopo — acesso por field.
        builder.HasMany(group => group.SelectedInsurers)
            .WithOne()
            .HasForeignKey(insurer => insurer.QuotationGroupId)
            .IsRequired();

        builder.Navigation(group => group.SelectedInsurers)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // Alinhado 1:1 com a migration criar-tabela-quotation-groups (evitar drift de constraint).
        builder.Property(group => group.CreatedAt).IsRequired();
        builder.Property(group => group.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(group => group.UpdatedBy).HasMaxLength(100);
    }
}

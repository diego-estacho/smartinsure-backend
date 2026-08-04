using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInsure.Core.Entities;

namespace SmartInsure.Infra.Data.Mappings;

/// <summary>
/// Mapeamento do Termo da Seguradora (RN-506): uma versão vigente por Seguradora — garantido por índice
/// único filtrado em IsActive; versões anteriores ficam para os aceites já registrados.
/// </summary>
public sealed class InsurerTermMapping : IEntityTypeConfiguration<InsurerTerm>
{
    public void Configure(EntityTypeBuilder<InsurerTerm> builder)
    {
        builder.ToTable("InsurerTerms");

        builder.HasKey(term => term.Id);

        builder.Property(term => term.Id).ValueGeneratedNever();

        builder.HasOne<Insurer>()
            .WithMany()
            .HasForeignKey(term => term.InsurerId);

        builder.Property(term => term.Content).IsRequired();

        builder.Property(term => term.IsActive).IsRequired();

        builder.HasIndex(term => term.InsurerId)
            .HasDatabaseName("UX_InsurerTerms_ActiveByInsurer")
            .IsUnique()
            .HasFilter("[IsActive] = 1");

        builder.Property(term => term.CreatedAt).IsRequired();
        builder.Property(term => term.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(term => term.UpdatedBy).HasMaxLength(100);
    }
}

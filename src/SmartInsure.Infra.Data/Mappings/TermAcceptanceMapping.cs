using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInsure.Core.Entities;

namespace SmartInsure.Infra.Data.Mappings;

/// <summary>
/// Mapeamento do Aceite do Termo (RN-506): guarda o conteúdo aceito por extenso — é a prova do que foi
/// exibido, não uma referência que possa mudar de significado depois.
/// </summary>
public sealed class TermAcceptanceMapping : IEntityTypeConfiguration<TermAcceptance>
{
    public void Configure(EntityTypeBuilder<TermAcceptance> builder)
    {
        builder.ToTable("TermAcceptances");

        builder.HasKey(acceptance => acceptance.Id);

        builder.Property(acceptance => acceptance.Id).ValueGeneratedNever();

        builder.HasOne<InsurerTerm>()
            .WithMany()
            .HasForeignKey(acceptance => acceptance.InsurerTermId);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(acceptance => acceptance.UserId);

        builder.Property(acceptance => acceptance.AcceptedContent).IsRequired();
        builder.Property(acceptance => acceptance.UserAgent).HasMaxLength(400);
        builder.Property(acceptance => acceptance.AcceptedAt).IsRequired();

        // Consultado por Usuário (o que esta pessoa aceitou) e por Termo (quem aceitou esta versão).
        builder.HasIndex(acceptance => acceptance.UserId);
        builder.HasIndex(acceptance => acceptance.InsurerTermId);

        builder.Property(acceptance => acceptance.CreatedAt).IsRequired();
        builder.Property(acceptance => acceptance.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(acceptance => acceptance.UpdatedBy).HasMaxLength(100);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInsure.Core.Entities;

namespace SmartInsure.Infra.Data.Mappings;

public sealed class ImportedModalityParticularClauseMapping : IEntityTypeConfiguration<ImportedModalityParticularClause>
{
    public void Configure(EntityTypeBuilder<ImportedModalityParticularClause> builder)
    {
        builder.ToTable("ImportedModalityParticularClauses");
        builder.HasKey(clause => clause.Id);

        builder.Property(clause => clause.ImportedModalityId).IsRequired();
        builder.Property(clause => clause.ExternalId).HasMaxLength(100).IsRequired();
        builder.Property(clause => clause.Name).HasMaxLength(300).IsRequired();
        builder.Property(clause => clause.ClauseText);
        builder.Property(clause => clause.JsonTag);
        builder.Property(clause => clause.Status).HasMaxLength(20).IsRequired();

        // RN-048: identidade por (Modalidade Importada, id externo da cláusula).
        builder.HasIndex(clause => new { clause.ImportedModalityId, clause.ExternalId }).IsUnique();
        builder.HasOne<ImportedModality>().WithMany().HasForeignKey(clause => clause.ImportedModalityId);

        builder.Property(clause => clause.CreatedAt).IsRequired();
        builder.Property(clause => clause.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(clause => clause.UpdatedBy).HasMaxLength(100);
    }
}

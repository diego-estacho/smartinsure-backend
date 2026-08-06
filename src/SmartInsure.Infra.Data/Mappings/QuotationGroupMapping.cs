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

        // Id é UUIDv7 gerado pela aplicação (EntityBase), não pelo banco — sem isso o EF trata
        // a chave preenchida como "já existe" e gera UPDATE em vez de INSERT ao adicionar filhos.
        builder.Property(group => group.Id).ValueGeneratedNever();

        // FKs com DeleteBehavior.Restrict (convenção global, ADR-034). Tomador e Segurado são Pessoas.
        builder.HasOne<Person>()
            .WithMany()
            .HasForeignKey(group => group.PolicyHolderId);

        // RN-102/ADR-101: estabelecimento cotado — Filial opcional; sem navegação (o design não pede).
        builder.HasOne<Person>()
            .WithMany()
            .HasForeignKey(group => group.BranchPersonId)
            .OnDelete(DeleteBehavior.NoAction);

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

        // RN-104: as Coberturas Adicionais escolhidas viraram coleção filha (ver HasMany abaixo);
        // os booleanos provisórios saíram do domínio e da tabela (AB#0007).

        builder.Property(group => group.Status)
            .HasMaxLength(20)
            .IsRequired();

        // RN-059: a Cotação escolhida do Grupo — FK opcional, Restrict pela convenção global (ADR-034).
        builder.HasOne<Quotation>()
            .WithMany()
            .HasForeignKey(group => group.SelectedQuotationId)
            .IsRequired(false);

        // RN-057/ADR-050: a Corretora da última solicitação — FK opcional a Pessoa (Restrict, ADR-034);
        // usada pelo reconciliador para reconstruir o work item do fan-out após restart.
        builder.HasOne<Person>()
            .WithMany()
            .HasForeignKey(group => group.BrokerageId)
            .IsRequired(false);

        // Histórico consultável por tomador e por segurado.
        builder.HasIndex(group => group.PolicyHolderId);
        builder.HasIndex(group => group.InsuredId);

        // Filtrado: espelha a migration Flyway (só grupos com Filial cotada têm BranchPersonId preenchido).
        builder.HasIndex(group => group.BranchPersonId)
            .HasDatabaseName("IX_QuotationGroups_BranchPersonId")
            .HasFilter("[BranchPersonId] IS NOT NULL");

        // RN-503: réplica do endereço do Segurado — 1:1 com a oferta, carregada junto (a emissão precisa).
        builder.HasOne(group => group.InsuredAddress)
            .WithOne()
            .HasForeignKey<QuotationAddress>(address => address.QuotationGroupId)
            .IsRequired(false);

        // Coleção filha das Seguradoras do escopo — acesso por field.
        builder.HasMany(group => group.SelectedInsurers)
            .WithOne()
            .HasForeignKey(insurer => insurer.QuotationGroupId)
            .IsRequired();

        builder.Navigation(group => group.SelectedInsurers)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // RN-104: coleção filha das Coberturas Adicionais escolhidas — acesso por field.
        builder.HasMany(group => group.AdditionalCoverages)
            .WithOne()
            .HasForeignKey(coverage => coverage.QuotationGroupId)
            .IsRequired();

        builder.Navigation(group => group.AdditionalCoverages)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // Alinhado 1:1 com a migration criar-tabela-quotation-groups (evitar drift de constraint).
        builder.Property(group => group.CreatedAt).IsRequired();
        builder.Property(group => group.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(group => group.UpdatedBy).HasMaxLength(100);
    }
}

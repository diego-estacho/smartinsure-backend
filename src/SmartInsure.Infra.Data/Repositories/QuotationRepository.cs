using Microsoft.EntityFrameworkCore;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Infra.Data.Context;

namespace SmartInsure.Infra.Data.Repositories;

/// <summary>Repositório da Cotação (RN-057..063). Conclusão da unidade de trabalho é do UseCase (ADR-036).</summary>
public sealed class QuotationRepository(SmartInsureDbContext dbContext) : IQuotationRepository
{
    public async Task<Quotation?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => await dbContext.Quotations
            .Include(quotation => quotation.Reasons)
            // RN-105/RN-106: o processor substitui esta coleção (RecordAdditionalCoverages limpa antes
            // de gravar) e o Remove precisa dela carregada para derrubar os filhos antes da raiz — a
            // FK é Restrict por convenção (ADR-034). Simétrico ao Include de Reasons.
            .Include(quotation => quotation.AdditionalCoverages)
            .FirstOrDefaultAsync(quotation => quotation.Id == id, cancellationToken);

    public async Task AddAsync(Quotation entity, CancellationToken cancellationToken)
        => await dbContext.Quotations.AddAsync(entity, cancellationToken);

    public void Update(Quotation entity) => dbContext.Quotations.Update(entity);

    public void Remove(Quotation entity)
    {
        // RN-060 (recálculo): a FK Cotação→Motivo é Restrict (ADR-034, convenção global sobrescreve o
        // mapping). Removemos os motivos carregados ANTES da Cotação para o EF não tentar orfanar a FK
        // obrigatória do filho (severed association). O chamador carrega a Cotação com os motivos.
        if (entity.Reasons.Count > 0)
        {
            dbContext.QuotationReasons.RemoveRange(entity.Reasons);
        }

        // RN-105/RN-106 (AB#0007): a situação das Coberturas Adicionais é filha da Cotação e cai na
        // mesma FK Restrict — sem removê-la antes, o recálculo passa a falhar. O chamador carrega a
        // Cotação com a coleção (ver ListByGroupAsync).
        if (entity.AdditionalCoverages.Count > 0)
        {
            dbContext.QuotationAdditionalCoverages.RemoveRange(entity.AdditionalCoverages);
        }

        dbContext.Quotations.Remove(entity);
    }

    /// <summary>RN-057: Cotações do Grupo (rastreadas) — usadas no acompanhamento e na re-solicitação.</summary>
    public async Task<IReadOnlyList<Quotation>> ListByGroupAsync(
        Guid quotationGroupId, CancellationToken cancellationToken)
        => await dbContext.Quotations
            .Include(quotation => quotation.Reasons)
            .Include(quotation => quotation.AdditionalCoverages)
            .Where(quotation => quotation.QuotationGroupId == quotationGroupId)
            .ToListAsync(cancellationToken);

    /// <summary>RN-060: existe Cotação no Grupo? Barato (EXISTS), usado para barrar a edição de Grupo cotado.</summary>
    public async Task<bool> ExistsForGroupAsync(Guid quotationGroupId, CancellationToken cancellationToken)
        => await dbContext.Quotations
            .AnyAsync(quotation => quotation.QuotationGroupId == quotationGroupId, cancellationToken);

    public async Task AddRangeAsync(IEnumerable<Quotation> quotations, CancellationToken cancellationToken)
        => await dbContext.Quotations.AddRangeAsync(quotations, cancellationToken);

    /// <summary>
    /// ADR-050: Cotações ainda em Requested cujo lease expirou — as que o restart deixou órfãs (a fila
    /// in-process é volátil). O lease é o instante de início do processamento; se nulo (nunca foi obtida),
    /// cai na idade da Cotação (CreatedAt). Junta com o Grupo para trazer a Corretora e montar o work item.
    /// Só as com Corretora conhecida (Grupos cotados após a persistência do BrokerageId).
    /// </summary>
    public async Task<IReadOnlyList<QuotationRequestWorkItem>> ListStaleRequestedWorkItemsAsync(
        DateTime staleBeforeUtc, CancellationToken cancellationToken)
        => await dbContext.Quotations
            .AsNoTracking()
            .Where(quotation => quotation.ProcessingStatus == EQuotationProcessingStatus.Requested
                                && (quotation.ProcessingStartedAt ?? quotation.CreatedAt) < staleBeforeUtc)
            .Join(
                dbContext.QuotationGroups.AsNoTracking(),
                quotation => quotation.QuotationGroupId,
                group => group.Id,
                (quotation, group) => new { quotation, group.BrokerageId })
            .Where(row => row.BrokerageId != null)
            .Select(row => new QuotationRequestWorkItem(
                row.quotation.Id,
                row.quotation.QuotationGroupId,
                row.quotation.InsurerId,
                row.BrokerageId!.Value))
            .ToListAsync(cancellationToken);
}

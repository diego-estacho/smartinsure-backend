using Microsoft.EntityFrameworkCore;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Repositories.Dtos;
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

        dbContext.Quotations.Remove(entity);
    }

    /// <summary>RN-057: Cotações do Grupo (rastreadas) — usadas no acompanhamento e na re-solicitação.</summary>
    public async Task<IReadOnlyList<Quotation>> ListByGroupAsync(
        Guid quotationGroupId, CancellationToken cancellationToken)
        => await dbContext.Quotations
            .Include(quotation => quotation.Reasons)
            .Where(quotation => quotation.QuotationGroupId == quotationGroupId)
            .ToListAsync(cancellationToken);

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

    /// <summary>
    /// RN-077/RN-078: livro de Cotações da Corretora. Inclui só as Obtained com resultado do provedor
    /// (não Unavailable, ou Unavailable com ao menos um motivo de origem Provider — exclui as locais
    /// "não incluída"); junta Grupo/Tomador/Segurado/Modalidade; a contagem por situação respeita a
    /// busca mas ignora a aba ativa. Nome/logo da Seguradora ficam para o use case resolver por id.
    /// </summary>
    public async Task<QuotationBookPageDto> ListBookAsync(
        Guid brokerageId,
        int page,
        int pageSize,
        string? search,
        EQuotationResult? situation,
        CancellationToken cancellationToken)
    {
        var query =
            from quotation in dbContext.Quotations.AsNoTracking()
            where quotation.ProcessingStatus == EQuotationProcessingStatus.Obtained
                  && (quotation.Result != EQuotationResult.Unavailable
                      || quotation.Reasons.Any(reason => reason.Source == EQuotationReasonSource.Provider))
            join grp in dbContext.QuotationGroups.AsNoTracking() on quotation.QuotationGroupId equals grp.Id
            where grp.BrokerageId == brokerageId
            join policyHolder in dbContext.Persons.AsNoTracking() on grp.PolicyHolderId equals policyHolder.Id
            join insured in dbContext.Persons.AsNoTracking() on grp.InsuredId equals insured.Id
            join modality in dbContext.Modalities.AsNoTracking() on grp.ModalityId equals modality.Id
            select new
            {
                Quotation = quotation,
                Group = grp,
                PolicyHolderName = policyHolder.Name,
                InsuredName = insured.Name,
                ModalityName = modality.Name,
            };

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(row =>
                (row.Quotation.ProposalNumber != null && row.Quotation.ProposalNumber.Contains(term))
                || row.PolicyHolderName.Contains(term)
                || row.InsuredName.Contains(term)
                || row.ModalityName.Contains(term));
        }

        // RN-078: contagem por situação sobre a busca corrente, ANTES de aplicar a aba (para as abas
        // mostrarem o total de cada situação). Chaves não-nulas: incluídas são sempre Obtained com resultado.
        var rawCounts = await query
            .GroupBy(row => row.Quotation.Result)
            .Select(group => new { group.Key, Count = group.LongCount() })
            .ToListAsync(cancellationToken);

        var counts = rawCounts
            .Where(entry => entry.Key.HasValue)
            .Select(entry => new QuotationSituationCountDto(entry.Key!.Value, entry.Count))
            .ToList();

        if (situation is { } situacao)
        {
            query = query.Where(row => row.Quotation.Result == situacao);
        }

        var totalCount = await query.LongCountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(row => row.Quotation.ObtainedAt ?? row.Quotation.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(row => new QuotationBookItemDto(
                row.Quotation.Id,
                row.Quotation.ProposalNumber,
                row.PolicyHolderName,
                row.InsuredName,
                row.Quotation.InsurerId,
                row.ModalityName,
                row.Group.InsuredAmount,
                row.Quotation.Premium,
                row.Quotation.CommissionPercentage,
                row.Quotation.Result!.Value,
                row.Group.CoverageStartDate,
                row.Group.CoverageEndDate,
                row.Quotation.CreatedAt))
            .ToListAsync(cancellationToken);

        return new QuotationBookPageDto(items, totalCount, counts);
    }
}

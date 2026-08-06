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

    /// <summary>
    /// RN-077/RN-078: livro de Cotações da Corretora. Inclui só as Obtained com resultado do provedor
    /// (não Unavailable, ou Unavailable com ao menos um motivo de origem Provider — exclui as locais
    /// "não incluída"); junta Grupo/Tomador/Segurado/Modalidade/Seguradora. Pipeline: base (escopo +
    /// inclusão) → opções (distintos no livro) → busca → filtros avançados → contagem por situação
    /// (ignora a aba) → aba de situação → total + página (ordenada por obtenção desc).
    /// </summary>
    public async Task<QuotationBookPageDto> ListBookAsync(
        QuotationBookFilter filter, CancellationToken cancellationToken)
    {
        var baseQuery =
            from quotation in dbContext.Quotations.AsNoTracking()
            where quotation.ProcessingStatus == EQuotationProcessingStatus.Obtained
                  // Inclusão como EXISTS correlacionado no DbSet (não a navegação `quotation.Reasons.Any`):
                  // o EF Core não traduz a navegação de coleção dentro deste `||` quando a base é depois
                  // materializada com `Distinct()` (opções de filtro) — o EXISTS explícito traduz sempre.
                  && (quotation.Result != EQuotationResult.Unavailable
                      || dbContext.QuotationReasons.Any(reason =>
                          reason.QuotationId == quotation.Id
                          && reason.Source == EQuotationReasonSource.Provider))
            join grp in dbContext.QuotationGroups.AsNoTracking() on quotation.QuotationGroupId equals grp.Id
            where grp.BrokerageId == filter.BrokerageId
            join policyHolder in dbContext.Persons.AsNoTracking() on grp.PolicyHolderId equals policyHolder.Id
            join insured in dbContext.Persons.AsNoTracking() on grp.InsuredId equals insured.Id
            join modality in dbContext.Modalities.AsNoTracking() on grp.ModalityId equals modality.Id
            join insurer in dbContext.Insurers.AsNoTracking() on quotation.InsurerId equals insurer.Id
            select new
            {
                Quotation = quotation,
                Group = grp,
                PolicyHolderName = policyHolder.Name,
                InsuredName = insured.Name,
                ModalityId = modality.Id,
                ModalityName = modality.Name,
                InsurerId = insurer.Id,
                InsurerName = insurer.CorporateName,
                InsurerLogoUrl = insurer.LogoUrl,
            };

        // Q10: opções de filtro = distintos presentes no livro da Corretora (independentes dos demais filtros).
        // Distinct sobre tipo ANÔNIMO (não sobre o DTO com construtor): o EF Core não traduz `Distinct()`
        // de uma projeção para um record/DTO com ctor — o anônimo tem igualdade estrutural e vira
        // `SELECT DISTINCT col1, col2`; só depois projetamos para o DTO.
        var insurers = await baseQuery
            .Select(row => new { Id = row.InsurerId, Name = row.InsurerName })
            .Distinct()
            .OrderBy(option => option.Name)
            .Select(option => new QuotationBookOptionDto(option.Id, option.Name))
            .ToListAsync(cancellationToken);

        var modalities = await baseQuery
            .Select(row => new { Id = row.ModalityId, Name = row.ModalityName })
            .Distinct()
            .OrderBy(option => option.Name)
            .Select(option => new QuotationBookOptionDto(option.Id, option.Name))
            .ToListAsync(cancellationToken);

        // Busca livre (número/Tomador/Segurado/Seguradora/Modalidade).
        var query = baseQuery;
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim();
            query = query.Where(row =>
                (row.Quotation.ProposalNumber != null && row.Quotation.ProposalNumber.Contains(term))
                || row.PolicyHolderName.Contains(term)
                || row.InsuredName.Contains(term)
                || row.InsurerName.Contains(term)
                || row.ModalityName.Contains(term));
        }

        // Filtros avançados (E lógico).
        if (filter.InsurerId is { } insurerId)
        {
            query = query.Where(row => row.InsurerId == insurerId);
        }

        if (filter.ModalityId is { } modalityId)
        {
            query = query.Where(row => row.ModalityId == modalityId);
        }

        if (filter.PremiumMin is { } premiumMin)
        {
            query = query.Where(row => row.Quotation.Premium >= premiumMin);
        }

        if (filter.PremiumMax is { } premiumMax)
        {
            query = query.Where(row => row.Quotation.Premium <= premiumMax);
        }

        if (filter.InsuredAmountMin is { } insuredAmountMin)
        {
            query = query.Where(row => row.Group.InsuredAmount >= insuredAmountMin);
        }

        if (filter.InsuredAmountMax is { } insuredAmountMax)
        {
            query = query.Where(row => row.Group.InsuredAmount <= insuredAmountMax);
        }

        // Período de criação: DateOnly → limites DateTime (fim exclusivo no dia seguinte).
        if (filter.CreatedFrom is { } createdFrom)
        {
            var from = createdFrom.ToDateTime(TimeOnly.MinValue);
            query = query.Where(row => row.Quotation.CreatedAt >= from);
        }

        if (filter.CreatedTo is { } createdTo)
        {
            var toExclusive = createdTo.AddDays(1).ToDateTime(TimeOnly.MinValue);
            query = query.Where(row => row.Quotation.CreatedAt < toExclusive);
        }

        if (filter.CoverageStartFrom is { } coverageFrom)
        {
            query = query.Where(row => row.Group.CoverageStartDate >= coverageFrom);
        }

        if (filter.CoverageStartTo is { } coverageTo)
        {
            query = query.Where(row => row.Group.CoverageStartDate <= coverageTo);
        }

        // RN-078: contagem por situação sobre a busca + filtros avançados, ANTES da aba (para as abas
        // mostrarem o total de cada situação). Chaves não-nulas: incluídas são sempre Obtained com resultado.
        var rawCounts = await query
            .GroupBy(row => row.Quotation.Result)
            .Select(group => new { group.Key, Count = group.LongCount() })
            .ToListAsync(cancellationToken);

        var counts = rawCounts
            .Where(entry => entry.Key.HasValue)
            .Select(entry => new QuotationSituationCountDto(entry.Key!.Value, entry.Count))
            .ToList();

        if (filter.Situation is { } situacao)
        {
            query = query.Where(row => row.Quotation.Result == situacao);
        }

        var totalCount = await query.LongCountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(row => row.Quotation.ObtainedAt ?? row.Quotation.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(row => new QuotationBookItemDto(
                row.Quotation.Id,
                row.Quotation.ProposalNumber,
                row.PolicyHolderName,
                row.InsuredName,
                row.InsurerId,
                row.InsurerName,
                row.InsurerLogoUrl,
                row.ModalityId,
                row.ModalityName,
                row.Group.InsuredAmount,
                row.Quotation.Premium,
                row.Quotation.CommissionPercentage,
                row.Quotation.Result!.Value,
                row.Quotation.RequiresCcg,
                row.Group.CoverageStartDate,
                row.Group.CoverageEndDate,
                row.Quotation.CreatedAt))
            .ToListAsync(cancellationToken);

        return new QuotationBookPageDto(items, totalCount, counts, insurers, modalities);
    }

    /// <summary>
    /// RN-081: detalhe de uma Cotação, escopado pela Corretora do Grupo e restrito à mesma inclusão do
    /// livro (RN-077: Obtained com resultado do provedor). Duas leituras baratas (uma linha): os escalares
    /// achatados e, se houver, a situação das Coberturas Adicionais com o nome canônico (RN-106). Escopo
    /// ou inexistência devolvem null indistinguível — o 404 do use case não revela existência.
    /// </summary>
    public async Task<QuotationDetailDto?> GetDetailAsync(
        Guid quotationId, Guid brokerageId, CancellationToken cancellationToken)
    {
        var row = await (
            from quotation in dbContext.Quotations.AsNoTracking()
            where quotation.Id == quotationId
                  && quotation.ProcessingStatus == EQuotationProcessingStatus.Obtained
                  && (quotation.Result != EQuotationResult.Unavailable
                      || dbContext.QuotationReasons.Any(reason =>
                          reason.QuotationId == quotation.Id
                          && reason.Source == EQuotationReasonSource.Provider))
            join grp in dbContext.QuotationGroups.AsNoTracking() on quotation.QuotationGroupId equals grp.Id
            where grp.BrokerageId == brokerageId
            join policyHolder in dbContext.Persons.AsNoTracking() on grp.PolicyHolderId equals policyHolder.Id
            join insured in dbContext.Persons.AsNoTracking() on grp.InsuredId equals insured.Id
            join modality in dbContext.Modalities.AsNoTracking() on grp.ModalityId equals modality.Id
            join insurer in dbContext.Insurers.AsNoTracking() on quotation.InsurerId equals insurer.Id
            select new
            {
                Quotation = quotation,
                Group = grp,
                PolicyHolderName = policyHolder.Name,
                PolicyHolderDocumentNumber = policyHolder.DocumentNumber,
                InsuredName = insured.Name,
                InsuredDocumentNumber = insured.DocumentNumber,
                InsurerId = insurer.Id,
                InsurerName = insurer.CorporateName,
                InsurerLogoUrl = insurer.LogoUrl,
                ModalityId = modality.Id,
                ModalityName = modality.Name,
            }).FirstOrDefaultAsync(cancellationToken);

        if (row is null)
        {
            return null;
        }

        // RN-106: nome canônico (para exibir mesmo a não contemplada, que não tem SentName) + situação.
        var additionalCoverages = await (
            from coverage in dbContext.QuotationAdditionalCoverages.AsNoTracking()
            join canonical in dbContext.AdditionalCoverages.AsNoTracking()
                on coverage.AdditionalCoverageId equals canonical.Id
            where coverage.QuotationId == quotationId
            orderby canonical.Name
            select new QuotationDetailCoverageDto(canonical.Name, coverage.Status, coverage.SentName))
            .ToListAsync(cancellationToken);

        return new QuotationDetailDto(
            row.Quotation.Id,
            row.Quotation.ProposalNumber,
            row.PolicyHolderName,
            row.PolicyHolderDocumentNumber,
            row.InsuredName,
            row.InsuredDocumentNumber,
            row.InsurerId,
            row.InsurerName,
            row.InsurerLogoUrl,
            row.ModalityId,
            row.ModalityName,
            row.Group.InsuredAmount,
            row.Quotation.Premium,
            row.Quotation.CommissionPercentage,
            row.Quotation.CommissionValue,
            row.Group.CoverageStartDate,
            row.Group.CoverageEndDate,
            row.Quotation.CreatedAt,
            row.Quotation.ObtainedAt,
            row.Quotation.Result!.Value,
            row.Quotation.RequiresCcg,
            row.Quotation.CcgSigned,
            additionalCoverages);
    }
}

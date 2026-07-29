using Microsoft.EntityFrameworkCore;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Entities;
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
}

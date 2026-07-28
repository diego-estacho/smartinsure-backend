using Microsoft.EntityFrameworkCore;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Infra.Data.Context;

namespace SmartInsure.Infra.Data.Repositories;

/// <summary>Repositório de Cotações (RN-057..061). Agregado próprio, persistido por Seguradora.</summary>
public sealed class QuotationRepository(SmartInsureDbContext dbContext) : IQuotationRepository
{
    public async Task AddRangeAsync(IEnumerable<Quotation> quotations, CancellationToken cancellationToken)
    {
        await dbContext.Quotations.AddRangeAsync(quotations, cancellationToken);
    }

    public async Task<Quotation?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        // Rastreada (o consumidor grava o resultado); inclui os motivos para substituí-los.
        return await dbContext.Quotations
            .Include(quotation => quotation.Reasons)
            .FirstOrDefaultAsync(quotation => quotation.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Quotation>> ListByGroupAsync(
        Guid quotationGroupId, CancellationToken cancellationToken)
    {
        return await dbContext.Quotations
            .AsNoTracking()
            .Include(quotation => quotation.Reasons)
            .Where(quotation => quotation.QuotationGroupId == quotationGroupId)
            .OrderBy(quotation => quotation.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task RemoveByGroupAsync(Guid quotationGroupId, CancellationToken cancellationToken)
    {
        // RN-060: invalidação por recálculo. Exclusão composta explícita (ADR-034: sem cascade):
        // remove os motivos filhos antes das Cotações.
        var quotations = await dbContext.Quotations
            .Include(quotation => quotation.Reasons)
            .Where(quotation => quotation.QuotationGroupId == quotationGroupId)
            .ToListAsync(cancellationToken);

        if (quotations.Count == 0)
        {
            return;
        }

        dbContext.QuotationReasons.RemoveRange(quotations.SelectMany(quotation => quotation.Reasons));
        dbContext.Quotations.RemoveRange(quotations);
    }

    public async Task<IReadOnlyList<Quotation>> ListStaleRequestedAsync(
        DateTime olderThanUtc, CancellationToken cancellationToken)
    {
        return await dbContext.Quotations
            .AsNoTracking()
            .Where(quotation => quotation.ProcessingStatus == EQuotationProcessingStatus.Requested
                && quotation.CreatedAt < olderThanUtc)
            .ToListAsync(cancellationToken);
    }
}

using Microsoft.EntityFrameworkCore;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Infra.Data.Context;

namespace SmartInsure.Infra.Data.Repositories;

/// <summary>Repositório de Consultas de Crédito (RN-029..031).</summary>
public sealed class CreditInquiryRepository(SmartInsureDbContext dbContext) : ICreditInquiryRepository
{
    public async Task AddAsync(CreditInquiry inquiry, CancellationToken cancellationToken)
    {
        await dbContext.CreditInquiries.AddAsync(inquiry, cancellationToken);
    }

    public async Task<CreditInquiry?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await dbContext.CreditInquiries
            .AsNoTracking()
            .Include(inquiry => inquiry.Results)
            .FirstOrDefaultAsync(inquiry => inquiry.Id == id, cancellationToken);
    }

    public async Task<(IReadOnlyList<CreditInquiryListItem> Items, long TotalCount)> ListPagedAsync(
        Guid? brokerageId,
        string? policyHolderCnpj,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var query = dbContext.CreditInquiries.AsNoTracking();

        if (brokerageId.HasValue)
        {
            query = query.Where(inquiry => inquiry.BrokerageId == brokerageId.Value);
        }

        if (!string.IsNullOrWhiteSpace(policyHolderCnpj))
        {
            // Normaliza o CNPJ removendo caracteres não-numéricos antes de comparar
            // (banco armazena CNPJ normalizado — RN-007)
            var normalizedCnpj = new string(policyHolderCnpj.Where(char.IsDigit).ToArray());
            query = query.Where(inquiry => inquiry.PolicyHolderCnpj.Contains(normalizedCnpj));
        }

        var total = await query.LongCountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(inquiry => inquiry.QueriedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(inquiry => new CreditInquiryListItem(
                inquiry.Id,
                inquiry.BrokerageId,
                inquiry.PolicyHolderCnpj,
                inquiry.QueriedAt,
                inquiry.Results.Count,
                inquiry.Results.Count(r => r.Status == ECreditInquiryResultStatus.Available)))
            .ToListAsync(cancellationToken);

        return (items, total);
    }
}

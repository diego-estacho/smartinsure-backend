using Microsoft.EntityFrameworkCore;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Entities;
using SmartInsure.Infra.Data.Context;

namespace SmartInsure.Infra.Data.Repositories;

/// <summary>Repositório da Apólice (RN-507/RN-514).</summary>
public sealed class PolicyRepository(SmartInsureDbContext context)
    : Repository<Policy>(context), IPolicyRepository
{
    public async Task<bool> ExistsForQuotationAsync(Guid quotationId, CancellationToken cancellationToken)
        => await Set.AsNoTracking().AnyAsync(policy => policy.QuotationId == quotationId, cancellationToken);
}

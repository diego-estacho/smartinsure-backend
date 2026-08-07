using Microsoft.EntityFrameworkCore;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Entities;
using SmartInsure.Infra.Data.Context;

namespace SmartInsure.Infra.Data.Repositories;

/// <summary>Repositório do Termo da Seguradora (RN-506).</summary>
public sealed class InsurerTermRepository(SmartInsureDbContext context)
    : Repository<InsurerTerm>(context), IInsurerTermRepository
{
    public async Task<InsurerTerm?> GetActiveByInsurerAsync(Guid insurerId, CancellationToken cancellationToken)
        => await Set.AsNoTracking()
            .FirstOrDefaultAsync(
                term => term.InsurerId == insurerId && term.IsActive,
                cancellationToken);
}

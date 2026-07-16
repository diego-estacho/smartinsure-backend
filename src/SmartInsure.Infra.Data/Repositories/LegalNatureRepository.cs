using Microsoft.EntityFrameworkCore;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Entities;
using SmartInsure.Infra.Data.Context;

namespace SmartInsure.Infra.Data.Repositories;

public sealed class LegalNatureRepository(SmartInsureDbContext context)
    : Repository<LegalNature>(context), ILegalNatureRepository
{
    public async Task<LegalNature?> GetByCodeAsync(string code, CancellationToken cancellationToken)
        => await Set.AsNoTracking()
            .FirstOrDefaultAsync(nature => nature.Code == code, cancellationToken);
}

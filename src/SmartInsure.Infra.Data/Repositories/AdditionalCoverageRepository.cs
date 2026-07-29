using Microsoft.EntityFrameworkCore;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Repositories.Dtos;
using SmartInsure.Core.Entities;
using SmartInsure.Infra.Data.Context;

namespace SmartInsure.Infra.Data.Repositories;

public sealed class AdditionalCoverageRepository(SmartInsureDbContext context)
    : Repository<AdditionalCoverage>(context), IAdditionalCoverageRepository
{
    public async Task<AdditionalCoverage?> GetByNameAsync(string name, CancellationToken cancellationToken)
        => await Set.FirstOrDefaultAsync(coverage => coverage.Name == name, cancellationToken);

    public async Task<IReadOnlyList<AdditionalCoverageListItemDto>> ListAllAsync(CancellationToken cancellationToken)
        => await Set.AsNoTracking()
            .OrderBy(coverage => coverage.Name)
            .Select(coverage => new AdditionalCoverageListItemDto(
                coverage.Id, coverage.Name, coverage.Status.ToString()))
            .ToListAsync(cancellationToken);
}

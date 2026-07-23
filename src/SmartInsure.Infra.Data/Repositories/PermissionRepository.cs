using Microsoft.EntityFrameworkCore;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Entities;
using SmartInsure.Infra.Data.Context;

namespace SmartInsure.Infra.Data.Repositories;

public sealed class PermissionRepository(SmartInsureDbContext context)
    : Repository<Permission>(context), IPermissionRepository
{
    public async Task<IReadOnlyCollection<Permission>> GetByCodesAsync(
        IEnumerable<string> codes, CancellationToken cancellationToken)
        => await Set.AsNoTracking()
            .Where(permission => codes.Contains(permission.Code))
            .ToListAsync(cancellationToken);
}

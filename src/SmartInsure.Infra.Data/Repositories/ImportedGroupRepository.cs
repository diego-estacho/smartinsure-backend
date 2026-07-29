using Microsoft.EntityFrameworkCore;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Entities;
using SmartInsure.Infra.Data.Context;

namespace SmartInsure.Infra.Data.Repositories;

public sealed class ImportedGroupRepository(SmartInsureDbContext context)
    : Repository<ImportedGroup>(context), IImportedGroupRepository
{
    public async Task<ImportedGroup?> GetByInsurerAndSourceAsync(
        Guid insurerId, string sourceId, CancellationToken cancellationToken)
        => await Set.FirstOrDefaultAsync(
            group => group.InsurerId == insurerId && group.SourceId == sourceId,
            cancellationToken);
}

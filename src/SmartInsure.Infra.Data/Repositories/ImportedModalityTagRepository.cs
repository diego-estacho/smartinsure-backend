using Microsoft.EntityFrameworkCore;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Entities;
using SmartInsure.Infra.Data.Context;

namespace SmartInsure.Infra.Data.Repositories;

public sealed class ImportedModalityTagRepository(SmartInsureDbContext context)
    : Repository<ImportedModalityTag>(context), IImportedModalityTagRepository
{
    public async Task<ImportedModalityTag?> GetByImportedModalityAsync(
        Guid importedModalityId, CancellationToken cancellationToken)
        => await Set.FirstOrDefaultAsync(
            tag => tag.ImportedModalityId == importedModalityId,
            cancellationToken);
}

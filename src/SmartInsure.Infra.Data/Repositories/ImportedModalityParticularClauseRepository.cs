using Microsoft.EntityFrameworkCore;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Entities;
using SmartInsure.Infra.Data.Context;

namespace SmartInsure.Infra.Data.Repositories;

public sealed class ImportedModalityParticularClauseRepository(SmartInsureDbContext context)
    : Repository<ImportedModalityParticularClause>(context), IImportedModalityParticularClauseRepository
{
    public async Task<IReadOnlyList<ImportedModalityParticularClause>> ListByImportedModalityAsync(
        Guid importedModalityId, CancellationToken cancellationToken)
        => await Set
            .Where(clause => clause.ImportedModalityId == importedModalityId)
            .ToListAsync(cancellationToken);
}

using Microsoft.EntityFrameworkCore;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Infra.Data.Context;

namespace SmartInsure.Infra.Data.Repositories;

public sealed class ModalityMappingRepository(SmartInsureDbContext context)
    : Repository<ModalityMapping>(context), IModalityMappingRepository
{
    public async Task<bool> HasConfirmedAsync(Guid importedModalityId, CancellationToken cancellationToken)
        => await Set.AsNoTracking()
            .AnyAsync(
                mapping => mapping.ImportedModalityId == importedModalityId
                    && mapping.Status == EModalityMappingStatus.Confirmed,
                cancellationToken);
}

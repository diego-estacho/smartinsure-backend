using Microsoft.EntityFrameworkCore;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Infra.Data.Context;

namespace SmartInsure.Infra.Data.Repositories;

public sealed class ImportedModalityRepository(SmartInsureDbContext context)
    : Repository<ImportedModality>(context), IImportedModalityRepository
{
    public async Task<ImportedModality?> GetByInsurerAndSourceAsync(
        Guid insurerId, string sourceId, CancellationToken cancellationToken)
        => await Set.FirstOrDefaultAsync(
            modality => modality.InsurerId == insurerId && modality.SourceId == sourceId,
            cancellationToken);

    public async Task<IReadOnlyList<ImportedModality>> ListActiveByInsurerAsync(
        Guid insurerId, CancellationToken cancellationToken)
        => await Set
            .Where(modality => modality.InsurerId == insurerId
                && modality.Status == EImportedModalityStatus.Active)
            .ToListAsync(cancellationToken);

    public async Task<Guid?> FindConfirmedModalityIdByEngineAsync(
        string engineModalityId, ESuretyBranch branch, CancellationToken cancellationToken)
    {
        var query =
            from mapping in Context.Set<ModalityMapping>().AsNoTracking()
            join imported in Context.Set<ImportedModality>().AsNoTracking()
                on mapping.ImportedModalityId equals imported.Id
            where mapping.Status == EModalityMappingStatus.Confirmed
                && imported.EngineModalityId == engineModalityId
                && imported.Branch == branch
            select (Guid?)mapping.ModalityId;

        return await query.FirstOrDefaultAsync(cancellationToken);
    }
}

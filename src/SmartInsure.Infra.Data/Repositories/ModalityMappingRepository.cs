using Microsoft.EntityFrameworkCore;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Repositories.Dtos;
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

    public async Task<IReadOnlyList<ConfirmedMappingDto>> ListConfirmedActiveAsync(CancellationToken cancellationToken)
        => await (
            from mapping in Set.AsNoTracking()
            where mapping.Status == EModalityMappingStatus.Confirmed
            join imported in Context.Set<ImportedModality>().AsNoTracking()
                on mapping.ImportedModalityId equals imported.Id
            where imported.Status == EImportedModalityStatus.Active
            join insurer in Context.Set<Insurer>().AsNoTracking()
                on imported.InsurerId equals insurer.Id
            select new ConfirmedMappingDto(
                mapping.ModalityId,
                insurer.Id,
                insurer.CorporateName,
                imported.Id,
                imported.OriginName,
                imported.Branch.ToString()))
            .ToListAsync(cancellationToken);
}

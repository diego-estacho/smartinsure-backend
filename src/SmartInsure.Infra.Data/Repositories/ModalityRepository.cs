using Microsoft.EntityFrameworkCore;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Repositories.Dtos;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Infra.Data.Context;

namespace SmartInsure.Infra.Data.Repositories;

public sealed class ModalityRepository(SmartInsureDbContext context)
    : Repository<Modality>(context), IModalityRepository
{
    public async Task<bool> NameExistsAsync(
        string name, Guid? exceptModalityId, CancellationToken cancellationToken)
        => await Set.AsNoTracking()
            .AnyAsync(
                modality => modality.Name == name
                    && (exceptModalityId == null || modality.Id != exceptModalityId),
                cancellationToken);

    public async Task<(IReadOnlyList<ModalityListItemDto> Items, long TotalCount)> ListAsync(
        int page, int pageSize, bool includeInactive, CancellationToken cancellationToken)
    {
        var query = Set.AsNoTracking();

        if (!includeInactive)
        {
            query = query.Where(modality => modality.Status == EModalityStatus.Active);
        }

        var totalCount = await query.LongCountAsync(cancellationToken);

        var items = await query
            .OrderBy(modality => modality.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Join(
                Context.Set<ModalityGroup>().AsNoTracking(),
                modality => modality.ModalityGroupId,
                group => group.Id,
                (modality, group) => new ModalityListItemDto(
                    modality.Id,
                    modality.Name,
                    modality.ModalityGroupId,
                    group.Name,
                    modality.Description,
                    modality.Status.ToString()))
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}

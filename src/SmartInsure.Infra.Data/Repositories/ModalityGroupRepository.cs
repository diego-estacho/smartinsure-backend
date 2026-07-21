using Microsoft.EntityFrameworkCore;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Repositories.Dtos;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Infra.Data.Context;

namespace SmartInsure.Infra.Data.Repositories;

public sealed class ModalityGroupRepository(SmartInsureDbContext context)
    : Repository<ModalityGroup>(context), IModalityGroupRepository
{
    public async Task<bool> NameExistsAsync(
        string name, Guid? exceptGroupId, CancellationToken cancellationToken)
        => await Set.AsNoTracking()
            .AnyAsync(
                group => group.Name == name
                    && (exceptGroupId == null || group.Id != exceptGroupId),
                cancellationToken);

    public async Task<(IReadOnlyList<ModalityGroupListItemDto> Items, long TotalCount)> ListAsync(
        int page, int pageSize, bool includeInactive, CancellationToken cancellationToken)
    {
        var query = Set.AsNoTracking();

        if (!includeInactive)
        {
            query = query.Where(group => group.Status == EModalityGroupStatus.Active);
        }

        var totalCount = await query.LongCountAsync(cancellationToken);

        var items = await query
            .OrderBy(group => group.DisplayOrder)
            .ThenBy(group => group.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(group => new ModalityGroupListItemDto(
                group.Id,
                group.Name,
                group.Description,
                group.DisplayOrder,
                group.Status.ToString()))
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}

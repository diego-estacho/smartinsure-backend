using Microsoft.EntityFrameworkCore;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Repositories.Dtos;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Infra.Data.Context;

namespace SmartInsure.Infra.Data.Repositories;

public sealed class InsurerRepository(SmartInsureDbContext context)
    : Repository<Insurer>(context), IInsurerRepository
{
    public async Task<bool> CnpjExistsAsync(
        string cnpj, Guid? exceptInsurerId, CancellationToken cancellationToken)
        => await Set.AsNoTracking()
            .AnyAsync(
                insurer => insurer.Cnpj == cnpj
                    && (exceptInsurerId == null || insurer.Id != exceptInsurerId),
                cancellationToken);

    public async Task<(IReadOnlyList<InsurerListItemDto> Items, long TotalCount)> ListAsync(
        int page, int pageSize, bool includeInactive, CancellationToken cancellationToken)
    {
        var query = Set.AsNoTracking();

        if (!includeInactive)
        {
            query = query.Where(insurer => insurer.Status == EInsurerStatus.Active);
        }

        var totalCount = await query.LongCountAsync(cancellationToken);

        var items = await query
            .OrderBy(insurer => insurer.CorporateName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(insurer => new InsurerListItemDto(
                insurer.Id,
                insurer.Cnpj,
                insurer.CorporateName,
                insurer.TradeName,
                insurer.LogoUrl,
                insurer.Status.ToString()))
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<Insurer?> GetTrackedByIdAsync(Guid id, CancellationToken cancellationToken)
        => await Set.FirstOrDefaultAsync(insurer => insurer.Id == id, cancellationToken);
}

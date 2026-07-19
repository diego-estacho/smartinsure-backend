using Microsoft.EntityFrameworkCore;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Repositories.Dtos;
using SmartInsure.Core.Entities;
using SmartInsure.Infra.Data.Context;

namespace SmartInsure.Infra.Data.Repositories;

public sealed class InsurerEnablementRepository(SmartInsureDbContext context)
    : Repository<InsurerEnablement>(context), IInsurerEnablementRepository
{
    public async Task<bool> PairExistsAsync(
        Guid brokerageId, Guid insurerId, CancellationToken cancellationToken)
        => await Set.AsNoTracking()
            .AnyAsync(
                enablement => enablement.BrokerageId == brokerageId
                    && enablement.InsurerId == insurerId,
                cancellationToken);

    public async Task<InsurerEnablement?> GetByPairAsync(
        Guid brokerageId, Guid insurerId, CancellationToken cancellationToken)
        => await Set.AsNoTracking()
            .SingleOrDefaultAsync(
                enablement => enablement.BrokerageId == brokerageId
                    && enablement.InsurerId == insurerId,
                cancellationToken);

    public async Task<InsurerEnablementDetailsDto?> GetDetailsByIdAsync(
        Guid id, CancellationToken cancellationToken)
        => await Set.AsNoTracking()
            .Where(enablement => enablement.Id == id)
            .Select(enablement => new InsurerEnablementDetailsDto(
                enablement.Id,
                enablement.BrokerageId,
                Context.Persons
                    .Where(person => person.Id == enablement.BrokerageId)
                    .Select(person => person.Name)
                    .First(),
                enablement.InsurerId,
                Context.Insurers
                    .Where(insurer => insurer.Id == enablement.InsurerId)
                    .Select(insurer => insurer.CorporateName)
                    .First(),
                enablement.CalculationEngine.ToString(),
                enablement.ConnectionParameters,
                enablement.Status.ToString()))
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<(IReadOnlyList<InsurerEnablementListItemDto> Items, long TotalCount)> ListAsync(
        Guid? brokerageId, int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = Set.AsNoTracking();

        if (brokerageId is not null)
        {
            query = query.Where(enablement => enablement.BrokerageId == brokerageId);
        }

        var totalCount = await query.LongCountAsync(cancellationToken);

        var items = await query
            .OrderBy(enablement => enablement.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(enablement => new InsurerEnablementListItemDto(
                enablement.Id,
                enablement.BrokerageId,
                Context.Persons
                    .Where(person => person.Id == enablement.BrokerageId)
                    .Select(person => person.Name)
                    .First(),
                enablement.InsurerId,
                Context.Insurers
                    .Where(insurer => insurer.Id == enablement.InsurerId)
                    .Select(insurer => insurer.CorporateName)
                    .First(),
                enablement.CalculationEngine.ToString(),
                enablement.Status.ToString()))
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}

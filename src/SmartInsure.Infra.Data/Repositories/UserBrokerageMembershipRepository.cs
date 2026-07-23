using Microsoft.EntityFrameworkCore;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Entities;
using SmartInsure.Infra.Data.Context;

namespace SmartInsure.Infra.Data.Repositories;

public sealed class UserBrokerageMembershipRepository(SmartInsureDbContext context)
    : Repository<UserBrokerageMembership>(context), IUserBrokerageMembershipRepository
{
    public async Task<IReadOnlyCollection<UserBrokerageMembership>> GetByUserAsync(
        Guid userId, CancellationToken cancellationToken)
        => await Set.AsNoTracking()
            .Where(membership => membership.UserId == userId)
            .ToListAsync(cancellationToken);

    public async Task<bool> ExistsAsync(
        Guid userId, Guid brokerageId, CancellationToken cancellationToken)
        => await Set.AsNoTracking()
            .AnyAsync(
                membership => membership.UserId == userId && membership.BrokerageId == brokerageId,
                cancellationToken);
}

using Microsoft.EntityFrameworkCore;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Entities;
using SmartInsure.Infra.Data.Context;

namespace SmartInsure.Infra.Data.Repositories;

public sealed class UserPolicyHolderMembershipRepository(SmartInsureDbContext context)
    : Repository<UserPolicyHolderMembership>(context), IUserPolicyHolderMembershipRepository
{
    public async Task<IReadOnlyCollection<UserPolicyHolderMembership>> GetByUserAsync(
        Guid userId, CancellationToken cancellationToken)
        => await Set.AsNoTracking()
            .Where(membership => membership.UserId == userId)
            .ToListAsync(cancellationToken);

    public async Task<bool> ExistsAsync(
        Guid userId, Guid policyHolderId, CancellationToken cancellationToken)
        => await Set.AsNoTracking()
            .AnyAsync(
                membership => membership.UserId == userId && membership.PolicyHolderId == policyHolderId,
                cancellationToken);
}

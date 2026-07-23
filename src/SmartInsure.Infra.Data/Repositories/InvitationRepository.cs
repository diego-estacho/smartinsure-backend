using Microsoft.EntityFrameworkCore;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Entities;
using SmartInsure.Infra.Data.Context;

namespace SmartInsure.Infra.Data.Repositories;

public sealed class InvitationRepository(SmartInsureDbContext context)
    : Repository<Invitation>(context), IInvitationRepository
{
    public async Task<Invitation?> GetPendingByUserAsync(Guid userId, CancellationToken cancellationToken)
        => await Set.AsNoTracking()
            .FirstOrDefaultAsync(
                invitation => invitation.UserId == userId && invitation.ConsumedAtUtc == null,
                cancellationToken);

    public async Task<Invitation?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken)
        => await Set.AsNoTracking()
            .FirstOrDefaultAsync(invitation => invitation.TokenHash == tokenHash, cancellationToken);
}

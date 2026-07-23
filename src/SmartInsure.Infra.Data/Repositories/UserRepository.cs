using Microsoft.EntityFrameworkCore;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Entities;
using SmartInsure.Infra.Data.Context;

namespace SmartInsure.Infra.Data.Repositories;

public sealed class UserRepository(SmartInsureDbContext context)
    : Repository<User>(context), IUserRepository
{
    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken)
        => await Set.AsNoTracking()
            .AnyAsync(user => user.Email == email, cancellationToken);

    public async Task<User?> GetByExternalIdentityAsync(
        string externalIdentity, CancellationToken cancellationToken)
        => await Set.AsNoTracking()
            .Include(user => user.Profile)
            .FirstOrDefaultAsync(user => user.ExternalIdentity == externalIdentity, cancellationToken);

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken)
        => await Set.AsNoTracking()
            .FirstOrDefaultAsync(user => user.Email == email, cancellationToken);

    public async Task<int> CountByProfileIdAsync(
        Guid profileId, CancellationToken cancellationToken)
        => await Set.AsNoTracking()
            .CountAsync(user => user.ProfileId == profileId, cancellationToken);
}

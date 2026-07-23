using Microsoft.EntityFrameworkCore;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Constants;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Infra.Data.Context;

namespace SmartInsure.Infra.Data.Repositories;

public sealed class ProfileRepository(SmartInsureDbContext context)
    : Repository<Profile>(context), IProfileRepository
{
    public async Task<Profile?> GetByNameAsync(string name, CancellationToken cancellationToken)
        => await Set.AsNoTracking()
            .FirstOrDefaultAsync(profile => profile.Name == name, cancellationToken);

    public async Task<Profile?> GetSystemAdministratorAsync(CancellationToken cancellationToken)
        => await Set.AsNoTracking()
            .FirstOrDefaultAsync(
                profile => profile.IsFixed
                    && profile.Scope == EProfileScope.System
                    && profile.Name == ProfileNames.SystemAdministrator,
                cancellationToken);

    public async Task<Profile?> GetBrokerageAdministratorAsync(CancellationToken cancellationToken)
        => await Set.AsNoTracking()
            .FirstOrDefaultAsync(
                profile => profile.IsFixed
                    && profile.Scope == EProfileScope.Brokerage
                    && profile.Name == ProfileNames.BrokerageAdministrator,
                cancellationToken);
}

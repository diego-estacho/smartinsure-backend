using Microsoft.EntityFrameworkCore;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Repositories.Dtos;
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

    public async Task<(IReadOnlyList<ProfileListItemDto> Items, long TotalCount)> ListAsync(
        int page,
        int pageSize,
        string? search,
        EProfileScope? scope,
        CancellationToken cancellationToken)
    {
        var query = Set.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchTerm = search.Trim();
            query = query.Where(profile => profile.Name.Contains(searchTerm));
        }

        if (scope is not null)
        {
            query = query.Where(profile => profile.Scope == scope.Value);
        }

        var totalCount = await query.LongCountAsync(cancellationToken);

        var rows = await query
            .OrderBy(profile => profile.Scope)
            .ThenBy(profile => profile.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(profile => new
            {
                profile.Id,
                profile.Name,
                profile.Scope,
                profile.IsFixed,
                profile.BrokerageId,
                profile.PolicyHolderId,
                PermissionCount = profile.Permissions.Count,
            })
            .ToListAsync(cancellationToken);

        var items = rows
            .Select(row => new ProfileListItemDto(
                row.Id,
                row.Name,
                row.Scope.ToString(),
                row.IsFixed,
                row.BrokerageId,
                row.PolicyHolderId,
                row.PermissionCount))
            .ToList();

        return (items, totalCount);
    }

    public async Task<ProfileDetailsDto?> GetDetailsByIdAsync(
        Guid profileId,
        CancellationToken cancellationToken)
    {
        var profile = await Set.AsNoTracking()
            .Where(candidate => candidate.Id == profileId)
            .Select(candidate => new
            {
                candidate.Id,
                candidate.Name,
                candidate.Scope,
                candidate.IsFixed,
                candidate.BrokerageId,
                candidate.PolicyHolderId,
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (profile is null)
        {
            return null;
        }

        // RN-063: as Permissões do Perfil são as marcadas do catálogo (hoje o catálogo nasce vazio).
        var permissions = await Context.Set<ProfilePermission>().AsNoTracking()
            .Where(profilePermission => profilePermission.ProfileId == profileId)
            .Join(
                Context.Set<Permission>().AsNoTracking(),
                profilePermission => profilePermission.PermissionId,
                permission => permission.Id,
                (_, permission) => new ProfilePermissionDto(
                    permission.Id,
                    permission.Code,
                    permission.Description,
                    permission.IsSystem))
            .OrderBy(permission => permission.Code)
            .ToListAsync(cancellationToken);

        return new ProfileDetailsDto(
            profile.Id,
            profile.Name,
            profile.Scope.ToString(),
            profile.IsFixed,
            profile.BrokerageId,
            profile.PolicyHolderId,
            permissions);
    }

    public async Task<IReadOnlyList<Profile>> ListByScopeAsync(
        EProfileScope scope,
        Guid? ownerId,
        CancellationToken cancellationToken)
    {
        // Permissões carregadas: a gestão mostra a contagem por Perfil (RN-062).
        var query = Set.AsNoTracking()
            .Include(profile => profile.Permissions)
            .Where(profile => profile.Scope == scope);

        query = scope switch
        {
            // RN-072: global (dono nulo) vale para todos; customizado vale só para o próprio dono.
            EProfileScope.Brokerage => query.Where(profile =>
                profile.BrokerageId == null || profile.BrokerageId == ownerId),
            EProfileScope.PolicyHolder => query.Where(profile =>
                profile.PolicyHolderId == null || profile.PolicyHolderId == ownerId),
            _ => query,
        };

        return await query
            .OrderBy(profile => profile.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByNameInScopeAsync(
        string name,
        EProfileScope scope,
        Guid? ownerId,
        Guid? excludedProfileId,
        CancellationToken cancellationToken)
    {
        var trimmed = name.Trim();

        var query = Set.AsNoTracking()
            .Where(profile => profile.Scope == scope && profile.Name == trimmed);

        query = scope switch
        {
            EProfileScope.Brokerage => query.Where(profile => profile.BrokerageId == ownerId),
            EProfileScope.PolicyHolder => query.Where(profile => profile.PolicyHolderId == ownerId),
            _ => query,
        };

        if (excludedProfileId is { } excluded)
        {
            query = query.Where(profile => profile.Id != excluded);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<Profile?> GetTrackedByIdAsync(Guid profileId, CancellationToken cancellationToken)
        => await Set
            .Include(profile => profile.Permissions)
            .FirstOrDefaultAsync(profile => profile.Id == profileId, cancellationToken);

    public async Task<int> CountUsersByProfileAsync(Guid profileId, CancellationToken cancellationToken)
    {
        var brokerageUsers = await Context.Set<UserBrokerageMembership>().AsNoTracking()
            .CountAsync(membership => membership.ProfileId == profileId, cancellationToken);

        var policyHolderUsers = await Context.Set<UserPolicyHolderMembership>().AsNoTracking()
            .CountAsync(membership => membership.ProfileId == profileId, cancellationToken);

        var systemUsers = await Context.Set<User>().AsNoTracking()
            .CountAsync(user => user.ProfileId == profileId, cancellationToken);

        return brokerageUsers + policyHolderUsers + systemUsers;
    }

    public async Task<Profile?> GetBrokerageAdministratorAsync(CancellationToken cancellationToken)
        => await Set.AsNoTracking()
            .FirstOrDefaultAsync(
                profile => profile.IsFixed
                    && profile.Scope == EProfileScope.Brokerage
                    && profile.Name == ProfileNames.BrokerageAdministrator,
                cancellationToken);
}

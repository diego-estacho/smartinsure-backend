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
                profile.Description,
                profile.CreatedAt,
            })
            .ToListAsync(cancellationToken);

        // RN-074: uso (usuários + áreas) da página em poucas consultas, evitando N+1.
        var usage = await GetUsageAsync(rows.Select(row => row.Id).ToList(), cancellationToken);

        var items = rows
            .Select(row =>
            {
                var use = usage.TryGetValue(row.Id, out var value) ? value : new ProfileUsageDto(0, 0);
                return new ProfileListItemDto(
                    row.Id,
                    row.Name,
                    row.Scope.ToString(),
                    row.IsFixed,
                    row.BrokerageId,
                    row.PolicyHolderId,
                    row.PermissionCount,
                    row.Description,
                    row.CreatedAt,
                    use.UserCount,
                    use.AreaCount);
            })
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
                candidate.Description,
                candidate.CreatedAt,
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (profile is null)
        {
            return null;
        }

        // RN-063: as Permissões marcadas do catálogo. Projeta anônimo e ordena/mapeia em memória — o
        // EF não traduz OrderBy sobre a propriedade de um record já projetado no Join.
        var permissionRows = await Context.Set<ProfilePermission>().AsNoTracking()
            .Where(profilePermission => profilePermission.ProfileId == profileId)
            .Join(
                Context.Set<Permission>().AsNoTracking(),
                profilePermission => profilePermission.PermissionId,
                permission => permission.Id,
                (_, permission) => new
                {
                    permission.Id,
                    permission.Code,
                    permission.Description,
                    permission.IsSystem,
                })
            .ToListAsync(cancellationToken);

        var permissions = permissionRows
            .OrderBy(permission => permission.Code)
            .Select(permission => new ProfilePermissionDto(
                permission.Id,
                permission.Code,
                permission.Description,
                permission.IsSystem))
            .ToList();

        // RN-074: "Quem usa este perfil" — Usuários vinculados por qualquer Escopo (prévia + total).
        // Três consultas simples + merge em memória: o Union de Joins não traduz no EF e os vínculos
        // por Perfil são poucos. Dedup por Usuário (um Usuário aparece uma vez, mesmo em vários vínculos).
        const int linkedUsersPreview = 5;

        var brokerageUsers = await Context.Set<UserBrokerageMembership>().AsNoTracking()
            .Where(membership => membership.ProfileId == profileId)
            .Join(
                Context.Set<User>().AsNoTracking(),
                membership => membership.UserId,
                user => user.Id,
                (_, user) => new ProfileLinkedUserDto(user.Id, user.Name, user.Email))
            .ToListAsync(cancellationToken);

        var policyHolderUsers = await Context.Set<UserPolicyHolderMembership>().AsNoTracking()
            .Where(membership => membership.ProfileId == profileId)
            .Join(
                Context.Set<User>().AsNoTracking(),
                membership => membership.UserId,
                user => user.Id,
                (_, user) => new ProfileLinkedUserDto(user.Id, user.Name, user.Email))
            .ToListAsync(cancellationToken);

        var systemUsers = await Context.Set<User>().AsNoTracking()
            .Where(user => user.ProfileId == profileId)
            .Select(user => new ProfileLinkedUserDto(user.Id, user.Name, user.Email))
            .ToListAsync(cancellationToken);

        var linkedAll = brokerageUsers
            .Concat(policyHolderUsers)
            .Concat(systemUsers)
            .GroupBy(user => user.Id)
            .Select(group => group.First())
            .OrderBy(user => user.Name)
            .ToList();

        var linkedUserCount = linkedAll.Count;
        var linkedUsers = linkedAll.Take(linkedUsersPreview).ToList();

        return new ProfileDetailsDto(
            profile.Id,
            profile.Name,
            profile.Scope.ToString(),
            profile.IsFixed,
            profile.BrokerageId,
            profile.PolicyHolderId,
            permissions,
            profile.Description,
            profile.CreatedAt,
            linkedUsers,
            linkedUserCount);
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

    public void RemoveWithPermissions(Profile profile)
    {
        // RN-074: as FKs são Restrict (sem cascade), então os ProfilePermissions do perfil são
        // apagados explicitamente antes do pai — senão o EF acusa relação obrigatória rompida.
        Context.Set<ProfilePermission>().RemoveRange(profile.Permissions);
        Set.Remove(profile);
    }

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

    public async Task<IReadOnlyDictionary<Guid, ProfileUsageDto>> GetUsageAsync(
        IReadOnlyCollection<Guid> profileIds,
        CancellationToken cancellationToken)
    {
        if (profileIds.Count == 0)
        {
            return new Dictionary<Guid, ProfileUsageDto>();
        }

        var ids = profileIds.Distinct().ToList();

        var brokerageCounts = await Context.Set<UserBrokerageMembership>().AsNoTracking()
            .Where(membership => ids.Contains(membership.ProfileId))
            .GroupBy(membership => membership.ProfileId)
            .Select(group => new { ProfileId = group.Key, Count = group.Count() })
            .ToListAsync(cancellationToken);

        var policyHolderCounts = await Context.Set<UserPolicyHolderMembership>().AsNoTracking()
            .Where(membership => ids.Contains(membership.ProfileId))
            .GroupBy(membership => membership.ProfileId)
            .Select(group => new { ProfileId = group.Key, Count = group.Count() })
            .ToListAsync(cancellationToken);

        var systemCounts = await Context.Set<User>().AsNoTracking()
            .Where(user => user.ProfileId != null && ids.Contains(user.ProfileId.Value))
            .GroupBy(user => user.ProfileId!.Value)
            .Select(group => new { ProfileId = group.Key, Count = group.Count() })
            .ToListAsync(cancellationToken);

        var users = new Dictionary<Guid, int>();
        foreach (var row in brokerageCounts.Concat(policyHolderCounts).Concat(systemCounts))
        {
            users[row.ProfileId] = users.GetValueOrDefault(row.ProfileId) + row.Count;
        }

        // RN-074: nº de Áreas distintas que o Perfil toca (contagem no cabeçalho da listagem).
        var areaRows = await Context.Set<ProfilePermission>().AsNoTracking()
            .Where(profilePermission => ids.Contains(profilePermission.ProfileId))
            .Join(
                Context.Set<Permission>().AsNoTracking(),
                profilePermission => profilePermission.PermissionId,
                permission => permission.Id,
                (profilePermission, permission) => new { profilePermission.ProfileId, permission.Area })
            .Where(row => row.Area != null)
            .Distinct()
            .GroupBy(row => row.ProfileId)
            .Select(group => new { ProfileId = group.Key, Count = group.Count() })
            .ToListAsync(cancellationToken);

        var areas = areaRows.ToDictionary(row => row.ProfileId, row => row.Count);

        return ids.ToDictionary(
            id => id,
            id => new ProfileUsageDto(
                users.GetValueOrDefault(id),
                areas.GetValueOrDefault(id)));
    }

    public async Task ReassignMembershipsAsync(
        Guid fromProfileId,
        Guid toProfileId,
        EProfileScope scope,
        CancellationToken cancellationToken)
    {
        // Perfil removível é sempre customizado de Corretora ou Tomador (fixo não é removido, e o
        // Escopo System só tem Perfis fixos) — então só o tipo de Vínculo do Escopo é migrado.
        if (scope == EProfileScope.Brokerage)
        {
            var memberships = await Context.Set<UserBrokerageMembership>()
                .Where(membership => membership.ProfileId == fromProfileId)
                .ToListAsync(cancellationToken);

            foreach (var membership in memberships)
            {
                membership.Reassign(toProfileId);
            }
        }
        else if (scope == EProfileScope.PolicyHolder)
        {
            var memberships = await Context.Set<UserPolicyHolderMembership>()
                .Where(membership => membership.ProfileId == fromProfileId)
                .ToListAsync(cancellationToken);

            foreach (var membership in memberships)
            {
                membership.Reassign(toProfileId);
            }
        }
    }

    public async Task<Profile?> GetBrokerageAdministratorAsync(CancellationToken cancellationToken)
        => await Set.AsNoTracking()
            .FirstOrDefaultAsync(
                profile => profile.IsFixed
                    && profile.Scope == EProfileScope.Brokerage
                    && profile.Name == ProfileNames.BrokerageAdministrator,
                cancellationToken);
}

using Microsoft.EntityFrameworkCore;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Repositories.Dtos;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
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

    public async Task<(IReadOnlyList<UserListItemDto> Items, long TotalCount, UserStatusCountsDto Counts)> ListAsync(
        int page,
        int pageSize,
        UserListFilters filters,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        // Escopo (RN-064) + busca + filtros avançados (§4) formam a base; as contagens das abas usam
        // esta base, sem o filtro de situação. O filtro de situação (incl. "Expirado") só recorta a página.
        var baseQuery = ApplyFilters(Set.AsNoTracking(), filters);

        var counts = new UserStatusCountsDto(
            All: await baseQuery.LongCountAsync(cancellationToken),
            Active: await baseQuery.LongCountAsync(user => user.Status == EUserStatus.Active, cancellationToken),
            PendingNotExpired: await baseQuery.LongCountAsync(
                user => user.Status == EUserStatus.Pending
                    && !Context.Set<Invitation>().Any(invitation => invitation.UserId == user.Id
                        && invitation.ConsumedAtUtc == null && invitation.ExpiresAtUtc < now),
                cancellationToken),
            Expired: await baseQuery.LongCountAsync(
                user => user.Status == EUserStatus.Pending
                    && Context.Set<Invitation>().Any(invitation => invitation.UserId == user.Id
                        && invitation.ConsumedAtUtc == null && invitation.ExpiresAtUtc < now),
                cancellationToken),
            Inactive: await baseQuery.LongCountAsync(user => user.Status == EUserStatus.Inactive, cancellationToken));

        var pageQuery = filters.Status switch
        {
            EUserListStatusFilter.Active => baseQuery.Where(user => user.Status == EUserStatus.Active),
            EUserListStatusFilter.Inactive => baseQuery.Where(user => user.Status == EUserStatus.Inactive),
            EUserListStatusFilter.Expired => baseQuery.Where(
                user => user.Status == EUserStatus.Pending
                    && Context.Set<Invitation>().Any(invitation => invitation.UserId == user.Id
                        && invitation.ConsumedAtUtc == null && invitation.ExpiresAtUtc < now)),
            EUserListStatusFilter.PendingNotExpired => baseQuery.Where(
                user => user.Status == EUserStatus.Pending
                    && !Context.Set<Invitation>().Any(invitation => invitation.UserId == user.Id
                        && invitation.ConsumedAtUtc == null && invitation.ExpiresAtUtc < now)),
            _ => baseQuery,
        };

        var totalCount = await pageQuery.LongCountAsync(cancellationToken);

        var rows = await pageQuery
            .OrderBy(user => user.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(user => new
            {
                user.Id,
                user.Name,
                user.Email,
                user.Status,
                SystemProfileName = user.Profile == null ? null : user.Profile.Name,
                SystemProfileScope = user.Profile == null ? (EProfileScope?)null : user.Profile.Scope,
                SystemProfileIsFixed = user.Profile != null && user.Profile.IsFixed,
                user.CreatedAt,
                user.LastAccessAtUtc,
            })
            .ToListAsync(cancellationToken);

        var pageIds = rows.Select(row => row.Id).ToList();

        // Perfil/vínculo do Usuário sem Perfil de Sistema vive no Vínculo (RN-064): resolvemos o
        // representativo em memória (batched) — o Escopo ativo, quando houver, escolhe o Vínculo.
        var representatives = await ResolveRepresentativesAsync(
            rows.Where(row => row.SystemProfileName == null).Select(row => row.Id).ToList(),
            filters.VisibleBrokerageId, filters.VisiblePolicyHolderId, cancellationToken);

        // "Expirado" é situação de exibição: Pendente com Convite ativo vencido (RN-065).
        var expiredUserIds = await Context.Set<Invitation>().AsNoTracking()
            .Where(invitation => pageIds.Contains(invitation.UserId)
                && invitation.ConsumedAtUtc == null
                && invitation.ExpiresAtUtc < now)
            .Select(invitation => invitation.UserId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var expiredSet = expiredUserIds.ToHashSet();

        var items = rows
            .Select(row =>
            {
                var hasSystemProfile = row.SystemProfileName != null;
                var representative = hasSystemProfile ? null : representatives.GetValueOrDefault(row.Id);

                return new UserListItemDto(
                    row.Id,
                    row.Name,
                    row.Email,
                    row.Status.ToString(),
                    ProfileName: hasSystemProfile ? row.SystemProfileName : representative?.ProfileName,
                    ProfileScope: hasSystemProfile ? row.SystemProfileScope?.ToString() : representative?.Scope,
                    ProfileIsFixed: hasSystemProfile ? row.SystemProfileIsFixed : representative?.IsFixed ?? false,
                    Link: hasSystemProfile ? null : representative?.Link,
                    row.CreatedAt,
                    InviteExpired: row.Status == EUserStatus.Pending && expiredSet.Contains(row.Id),
                    row.LastAccessAtUtc);
            })
            .ToList();

        return (items, totalCount, counts);
    }

    public async Task<UserDetailsDto?> GetDetailsByIdAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        var user = await Set.AsNoTracking()
            .Where(candidate => candidate.Id == userId)
            .Select(candidate => new
            {
                candidate.Id,
                candidate.Name,
                candidate.Email,
                candidate.DocumentNumber,
                candidate.Status,
                candidate.ProfileId,
                ProfileName = candidate.Profile == null ? null : candidate.Profile.Name,
                ProfileScope = candidate.Profile == null ? (EProfileScope?)null : candidate.Profile.Scope,
                ProfileIsFixed = candidate.Profile != null && candidate.Profile.IsFixed,
                candidate.CreatedAt,
                candidate.LastAccessAtUtc,
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null)
        {
            return null;
        }

        // Convite ativo (não consumido) do Usuário, quando houver (RN-065): enviado em / expira em.
        var invitation = await Context.Set<Invitation>().AsNoTracking()
            .Where(candidate => candidate.UserId == userId && candidate.ConsumedAtUtc == null)
            .OrderByDescending(candidate => candidate.CreatedAt)
            .Select(candidate => new { candidate.CreatedAt, candidate.ExpiresAtUtc })
            .FirstOrDefaultAsync(cancellationToken);

        var inviteExpired = user.Status == EUserStatus.Pending
            && invitation is not null
            && invitation.ExpiresAtUtc < now;

        var brokerageMemberships =
            await BrokerageMembershipsQuery(Context, userId).ToListAsync(cancellationToken);

        var policyHolderMemberships =
            await PolicyHolderMembershipsQuery(Context, userId).ToListAsync(cancellationToken);

        return new UserDetailsDto(
            user.Id,
            user.Name,
            user.Email,
            user.DocumentNumber,
            user.Status.ToString(),
            user.ProfileId,
            user.ProfileName,
            user.ProfileScope?.ToString(),
            user.ProfileIsFixed,
            user.CreatedAt,
            invitation?.CreatedAt,
            invitation?.ExpiresAtUtc,
            inviteExpired,
            user.LastAccessAtUtc,
            brokerageMemberships,
            policyHolderMemberships);
    }

    /// <summary>
    /// Base comum da listagem e das contagens: visibilidade por Escopo (RN-064) + busca +
    /// filtros avançados (§4: perfil, escopo, vínculo, data de cadastro). Tudo com `.Any` de um
    /// nível (mesmo padrão do filtro de Escopo) para o EF traduzir sem cair em client-eval.
    /// </summary>
    private IQueryable<User> ApplyFilters(IQueryable<User> query, UserListFilters filters)
    {
        if (filters.VisibleBrokerageId is { } visibleBrokerageId)
        {
            query = query.Where(user => Context.Set<UserBrokerageMembership>()
                .Any(membership => membership.UserId == user.Id
                    && membership.BrokerageId == visibleBrokerageId));
        }

        if (filters.VisiblePolicyHolderId is { } visiblePolicyHolderId)
        {
            query = query.Where(user => Context.Set<UserPolicyHolderMembership>()
                .Any(membership => membership.UserId == user.Id
                    && membership.PolicyHolderId == visiblePolicyHolderId));
        }

        if (!string.IsNullOrWhiteSpace(filters.Search))
        {
            var searchTerm = filters.Search.Trim();
            query = query.Where(user => user.Name.Contains(searchTerm)
                || user.Email.Contains(searchTerm)
                || (user.DocumentNumber != null && user.DocumentNumber.Contains(searchTerm))
                || (user.Profile != null && user.Profile.Name.Contains(searchTerm)));
        }

        if (filters.ProfileId is { } profileId)
        {
            query = query.Where(user => user.ProfileId == profileId
                || Context.Set<UserBrokerageMembership>()
                    .Any(membership => membership.UserId == user.Id && membership.ProfileId == profileId)
                || Context.Set<UserPolicyHolderMembership>()
                    .Any(membership => membership.UserId == user.Id && membership.ProfileId == profileId));
        }

        if (filters.Scope is { } scope)
        {
            query = scope switch
            {
                EProfileScope.System => query.Where(
                    user => user.Profile != null && user.Profile.Scope == EProfileScope.System),
                EProfileScope.Brokerage => query.Where(
                    user => Context.Set<UserBrokerageMembership>().Any(membership => membership.UserId == user.Id)),
                EProfileScope.PolicyHolder => query.Where(
                    user => Context.Set<UserPolicyHolderMembership>().Any(membership => membership.UserId == user.Id)),
                _ => query,
            };
        }

        if (filters.LinkId is { } linkId)
        {
            query = query.Where(user =>
                Context.Set<UserBrokerageMembership>()
                    .Any(membership => membership.UserId == user.Id && membership.BrokerageId == linkId)
                || Context.Set<UserPolicyHolderMembership>()
                    .Any(membership => membership.UserId == user.Id && membership.PolicyHolderId == linkId));
        }

        if (filters.RegisteredFrom is { } registeredFrom)
        {
            query = query.Where(user => user.CreatedAt >= registeredFrom);
        }

        if (filters.RegisteredTo is { } registeredTo)
        {
            query = query.Where(user => user.CreatedAt <= registeredTo);
        }

        return query;
    }

    private sealed record RepresentativeProfile(string ProfileName, string Scope, bool IsFixed, string Link);

    /// <summary>
    /// Perfil/vínculo representativo do Usuário sem Perfil de Sistema (RN-064): o do Escopo ativo,
    /// quando houver; senão o primeiro Vínculo de Corretora e, na falta, o primeiro de Tomador.
    /// </summary>
    private async Task<Dictionary<Guid, RepresentativeProfile>> ResolveRepresentativesAsync(
        IReadOnlyList<Guid> userIds,
        Guid? brokerageId,
        Guid? policyHolderId,
        CancellationToken cancellationToken)
    {
        var representatives = new Dictionary<Guid, RepresentativeProfile>();

        if (userIds.Count == 0)
        {
            return representatives;
        }

        if (policyHolderId is not { } scopedPolicyHolderId)
        {
            var brokerageRows = await (
                from membership in Context.Set<UserBrokerageMembership>().AsNoTracking()
                where userIds.Contains(membership.UserId)
                    && (brokerageId == null || membership.BrokerageId == brokerageId)
                join brokerage in Context.Set<Person>().AsNoTracking()
                    on membership.BrokerageId equals brokerage.Id
                join profile in Context.Set<Profile>().AsNoTracking()
                    on membership.ProfileId equals profile.Id
                orderby brokerage.Name
                select new { membership.UserId, profile.Name, profile.Scope, profile.IsFixed, Link = brokerage.Name })
                .ToListAsync(cancellationToken);

            foreach (var row in brokerageRows)
            {
                representatives.TryAdd(
                    row.UserId,
                    new RepresentativeProfile(row.Name, row.Scope.ToString(), row.IsFixed, row.Link));
            }
        }

        var stillMissing = userIds.Where(id => !representatives.ContainsKey(id)).ToList();

        if (stillMissing.Count > 0)
        {
            var policyHolderRows = await (
                from membership in Context.Set<UserPolicyHolderMembership>().AsNoTracking()
                where stillMissing.Contains(membership.UserId)
                    && (policyHolderId == null || membership.PolicyHolderId == policyHolderId)
                join policyHolder in Context.Set<Person>().AsNoTracking()
                    on membership.PolicyHolderId equals policyHolder.Id
                join profile in Context.Set<Profile>().AsNoTracking()
                    on membership.ProfileId equals profile.Id
                orderby policyHolder.Name
                select new { membership.UserId, profile.Name, profile.Scope, profile.IsFixed, Link = policyHolder.Name })
                .ToListAsync(cancellationToken);

            foreach (var row in policyHolderRows)
            {
                representatives.TryAdd(
                    row.UserId,
                    new RepresentativeProfile(row.Name, row.Scope.ToString(), row.IsFixed, row.Link));
            }
        }

        return representatives;
    }

    /// <summary>
    /// RN-064: o Escopo do vínculo de Corretora é uma Person; o Perfil é o daquele Escopo.
    /// Consulta isolada para o teste conseguir traduzi-la (ToQueryString) sem abrir conexão.
    /// </summary>
    internal static IQueryable<UserMembershipDto> BrokerageMembershipsQuery(
        SmartInsureDbContext context,
        Guid userId)
        => context.Set<UserBrokerageMembership>().AsNoTracking()
            .Where(membership => membership.UserId == userId)
            .Join(
                context.Set<Person>().AsNoTracking(),
                membership => membership.BrokerageId,
                brokerage => brokerage.Id,
                (membership, brokerage) => new { membership, brokerage })
            .Join(
                context.Set<Profile>().AsNoTracking(),
                joined => joined.membership.ProfileId,
                profile => profile.Id,
                (joined, profile) => new { joined.membership, joined.brokerage, profile })
            // Mesma ordem de antes (ScopeName do DTO É o Person.Name), só que ANTES de projetar:
            // o EF não traduz OrderBy por propriedade de DTO construído — mesmo caso do
            // PersonRepository. Ordenar depois do Select derrubava GET /me com 500.
            .OrderBy(joined => joined.brokerage.Name)
            .Select(joined => new UserMembershipDto(
                joined.membership.Id,
                joined.brokerage.Id,
                joined.brokerage.DocumentNumber,
                joined.brokerage.Name,
                joined.profile.Id,
                joined.profile.Name,
                joined.profile.Scope.ToString(),
                joined.profile.IsFixed));

    /// <summary>RN-064: mesma leitura da <see cref="BrokerageMembershipsQuery"/>, para Tomador.</summary>
    internal static IQueryable<UserMembershipDto> PolicyHolderMembershipsQuery(
        SmartInsureDbContext context,
        Guid userId)
        => context.Set<UserPolicyHolderMembership>().AsNoTracking()
            .Where(membership => membership.UserId == userId)
            .Join(
                context.Set<Person>().AsNoTracking(),
                membership => membership.PolicyHolderId,
                policyHolder => policyHolder.Id,
                (membership, policyHolder) => new { membership, policyHolder })
            .Join(
                context.Set<Profile>().AsNoTracking(),
                joined => joined.membership.ProfileId,
                profile => profile.Id,
                (joined, profile) => new { joined.membership, joined.policyHolder, profile })
            .OrderBy(joined => joined.policyHolder.Name)
            .Select(joined => new UserMembershipDto(
                joined.membership.Id,
                joined.policyHolder.Id,
                joined.policyHolder.DocumentNumber,
                joined.policyHolder.Name,
                joined.profile.Id,
                joined.profile.Name,
                joined.profile.Scope.ToString(),
                joined.profile.IsFixed));
}

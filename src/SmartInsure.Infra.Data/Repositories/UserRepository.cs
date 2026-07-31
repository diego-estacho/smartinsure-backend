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

    public async Task<(IReadOnlyList<UserListItemDto> Items, long TotalCount)> ListAsync(
        int page,
        int pageSize,
        string? search,
        EUserStatus? status,
        Guid? brokerageId,
        Guid? policyHolderId,
        CancellationToken cancellationToken)
    {
        var query = Set.AsNoTracking();

        // RN-064: fora do Escopo Sistema, o Usuário só vê quem tem Vínculo no Escopo ativo dele.
        if (brokerageId is { } scopedBrokerageId)
        {
            query = query.Where(user => Context.Set<UserBrokerageMembership>()
                .Any(membership => membership.UserId == user.Id
                    && membership.BrokerageId == scopedBrokerageId));
        }

        if (policyHolderId is { } scopedPolicyHolderId)
        {
            query = query.Where(user => Context.Set<UserPolicyHolderMembership>()
                .Any(membership => membership.UserId == user.Id
                    && membership.PolicyHolderId == scopedPolicyHolderId));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchTerm = search.Trim();
            query = query.Where(user => user.Name.Contains(searchTerm)
                || user.Email.Contains(searchTerm));
        }

        if (status is not null)
        {
            query = query.Where(user => user.Status == status.Value);
        }

        var totalCount = await query.LongCountAsync(cancellationToken);

        var rows = await query
            .OrderBy(user => user.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(user => new
            {
                user.Id,
                user.Name,
                user.Email,
                user.Status,
                ProfileName = user.Profile == null ? null : user.Profile.Name,
                user.CreatedAt,
            })
            .ToListAsync(cancellationToken);

        var items = rows
            .Select(row => new UserListItemDto(
                row.Id,
                row.Name,
                row.Email,
                row.Status.ToString(),
                row.ProfileName,
                row.CreatedAt))
            .ToList();

        return (items, totalCount);
    }

    public async Task<UserDetailsDto?> GetDetailsByIdAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var user = await Set.AsNoTracking()
            .Where(candidate => candidate.Id == userId)
            .Select(candidate => new
            {
                candidate.Id,
                candidate.Name,
                candidate.Email,
                candidate.Status,
                candidate.ProfileId,
                ProfileName = candidate.Profile == null ? null : candidate.Profile.Name,
                candidate.CreatedAt,
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null)
        {
            return null;
        }

        // RN-064: o Escopo do vínculo (Corretora/Tomador) é uma Person; o Perfil é o daquele Escopo.
        var brokerageMemberships = await Context.Set<UserBrokerageMembership>().AsNoTracking()
            .Where(membership => membership.UserId == userId)
            .Join(
                Context.Set<Person>().AsNoTracking(),
                membership => membership.BrokerageId,
                brokerage => brokerage.Id,
                (membership, brokerage) => new { membership, brokerage })
            .Join(
                Context.Set<Profile>().AsNoTracking(),
                joined => joined.membership.ProfileId,
                profile => profile.Id,
                (joined, profile) => new { joined.membership, joined.brokerage, profile })
            // Ordena pela coluna real (Name) ANTES de projetar: o EF Core não traduz OrderBy por
            // propriedade de um DTO já construído no Select (não enxerga através do construtor).
            .OrderBy(row => row.brokerage.Name)
            .Select(row => new UserMembershipDto(
                row.membership.Id,
                row.brokerage.Id,
                row.brokerage.DocumentNumber,
                row.brokerage.Name,
                row.profile.Id,
                row.profile.Name))
            .ToListAsync(cancellationToken);

        var policyHolderMemberships = await Context.Set<UserPolicyHolderMembership>().AsNoTracking()
            .Where(membership => membership.UserId == userId)
            .Join(
                Context.Set<Person>().AsNoTracking(),
                membership => membership.PolicyHolderId,
                policyHolder => policyHolder.Id,
                (membership, policyHolder) => new { membership, policyHolder })
            .Join(
                Context.Set<Profile>().AsNoTracking(),
                joined => joined.membership.ProfileId,
                profile => profile.Id,
                (joined, profile) => new { joined.membership, joined.policyHolder, profile })
            // Ordena pela coluna real (Name) ANTES de projetar (mesmo motivo do vínculo de Corretora).
            .OrderBy(row => row.policyHolder.Name)
            .Select(row => new UserMembershipDto(
                row.membership.Id,
                row.policyHolder.Id,
                row.policyHolder.DocumentNumber,
                row.policyHolder.Name,
                row.profile.Id,
                row.profile.Name))
            .ToListAsync(cancellationToken);

        return new UserDetailsDto(
            user.Id,
            user.Name,
            user.Email,
            user.Status.ToString(),
            user.ProfileId,
            user.ProfileName,
            user.CreatedAt,
            brokerageMemberships,
            policyHolderMemberships);
    }
}

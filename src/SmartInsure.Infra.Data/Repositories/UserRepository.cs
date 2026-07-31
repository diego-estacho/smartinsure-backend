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

        var brokerageMemberships =
            await BrokerageMembershipsQuery(Context, userId).ToListAsync(cancellationToken);

        var policyHolderMemberships =
            await PolicyHolderMembershipsQuery(Context, userId).ToListAsync(cancellationToken);

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
            // Ordenar pela coluna antes de projetar: OrderBy sobre membro do DTO não traduz.
            .OrderBy(joined => joined.brokerage.Name)
            .Select(joined => new UserMembershipDto(
                joined.membership.Id,
                joined.brokerage.Id,
                joined.brokerage.DocumentNumber,
                joined.brokerage.Name,
                joined.profile.Id,
                joined.profile.Name));

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
                joined.profile.Name));
}

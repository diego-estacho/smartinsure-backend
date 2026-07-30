using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.Services.Scopes;

/// <summary>
/// RN-064 — Escopo ativo a partir dos Vínculos do Usuário. Nunca aceita um Escopo em que o
/// Usuário não esteja vinculado: a decisão é do servidor (SECURITY.md).
/// </summary>
public sealed class ActiveScopeResolver(
    IUserBrokerageMembershipRepository brokerageMembershipRepository,
    IUserPolicyHolderMembershipRepository policyHolderMembershipRepository) : IActiveScopeResolver
{
    public async Task<ActiveScope> ResolveDefaultAsync(Guid userId, CancellationToken cancellationToken)
    {
        var brokerageMemberships = await brokerageMembershipRepository.GetByUserAsync(
            userId, cancellationToken);
        var policyHolderMemberships = await policyHolderMembershipRepository.GetByUserAsync(
            userId, cancellationToken);

        // RN-064 (casos limite): vínculo único é sempre o ativo, sem escolha. Com vários, o
        // servidor não escolhe por conta própria — a seleção é do Usuário.
        var brokerageId = brokerageMemberships.Count == 1
            ? brokerageMemberships.Single().BrokerageId
            : (Guid?)null;

        var policyHolderId = policyHolderMemberships.Count == 1
            ? policyHolderMemberships.Single().PolicyHolderId
            : (Guid?)null;

        return new ActiveScope(brokerageId, policyHolderId);
    }

    public async Task<ActiveScope> ResolveRequestedAsync(
        Guid userId,
        Guid? brokerageId,
        Guid? policyHolderId,
        CancellationToken cancellationToken)
    {
        if (brokerageId is { } requestedBrokerageId
            && !await brokerageMembershipRepository.ExistsAsync(
                userId, requestedBrokerageId, cancellationToken))
        {
            throw new BusinessRuleException("O usuário não está vinculado a esta corretora.");
        }

        if (policyHolderId is { } requestedPolicyHolderId
            && !await policyHolderMembershipRepository.ExistsAsync(
                userId, requestedPolicyHolderId, cancellationToken))
        {
            throw new BusinessRuleException("O usuário não está vinculado a este tomador.");
        }

        return new ActiveScope(brokerageId, policyHolderId);
    }
}

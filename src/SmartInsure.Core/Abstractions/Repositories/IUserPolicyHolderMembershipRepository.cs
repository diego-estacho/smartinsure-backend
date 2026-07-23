using SmartInsure.Core.Entities;

namespace SmartInsure.Core.Abstractions.Repositories;

public interface IUserPolicyHolderMembershipRepository : IRepository<UserPolicyHolderMembership>
{
    /// <summary>RN-034: vínculos de Tomador de um Usuário.</summary>
    Task<IReadOnlyCollection<UserPolicyHolderMembership>> GetByUserAsync(
        Guid userId, CancellationToken cancellationToken);

    /// <summary>RN-034: existe vínculo do Usuário com o Tomador? (par único).</summary>
    Task<bool> ExistsAsync(Guid userId, Guid policyHolderId, CancellationToken cancellationToken);
}

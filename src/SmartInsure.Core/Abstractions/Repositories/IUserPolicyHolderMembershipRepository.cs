using SmartInsure.Core.Entities;

namespace SmartInsure.Core.Abstractions.Repositories;

public interface IUserPolicyHolderMembershipRepository : IRepository<UserPolicyHolderMembership>
{
    /// <summary>RN-064: vínculos de Tomador de um Usuário.</summary>
    Task<IReadOnlyCollection<UserPolicyHolderMembership>> GetByUserAsync(
        Guid userId, CancellationToken cancellationToken);

    /// <summary>RN-064: existe vínculo do Usuário com o Tomador? (par único).</summary>
    Task<bool> ExistsAsync(Guid userId, Guid policyHolderId, CancellationToken cancellationToken);

    /// <summary>RN-064: o vínculo do Usuário naquele Tomador, portador do Perfil dele ali (RN-070).</summary>
    Task<UserPolicyHolderMembership?> GetByUserAndPolicyHolderAsync(
        Guid userId,
        Guid policyHolderId,
        CancellationToken cancellationToken);
}

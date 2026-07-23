namespace SmartInsure.Core.Entities;

/// <summary>
/// Vínculo do Usuário com um Tomador (RN-034), portador do Perfil do Usuário naquele Tomador.
/// Um Usuário pode ter vários; no máximo um por Tomador. O Tomador é uma Person (papel PolicyHolder).
/// </summary>
public sealed class UserPolicyHolderMembership : EntityBase
{
    private UserPolicyHolderMembership()
    {
    }

    public Guid UserId { get; private set; }

    public Guid PolicyHolderId { get; private set; }

    public Guid ProfileId { get; private set; }

    public static UserPolicyHolderMembership Create(Guid userId, Guid policyHolderId, Guid profileId)
        => new()
        {
            UserId = userId,
            PolicyHolderId = policyHolderId,
            ProfileId = profileId,
        };
}

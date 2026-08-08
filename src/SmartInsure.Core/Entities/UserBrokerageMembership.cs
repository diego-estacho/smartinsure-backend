namespace SmartInsure.Core.Entities;

/// <summary>
/// Vínculo do Usuário com uma Corretora (RN-064), portador do Perfil do Usuário naquela Corretora.
/// Um Usuário pode ter vários; no máximo um por Corretora. A Corretora é uma Person (papel Broker).
/// </summary>
public sealed class UserBrokerageMembership : EntityBase
{
    private UserBrokerageMembership()
    {
    }

    public Guid UserId { get; private set; }

    public Guid BrokerageId { get; private set; }

    public Guid ProfileId { get; private set; }

    public static UserBrokerageMembership Create(Guid userId, Guid brokerageId, Guid profileId)
        => new()
        {
            UserId = userId,
            BrokerageId = brokerageId,
            ProfileId = profileId,
        };

    /// <summary>RN-074/RN-075: migra o Vínculo para outro Perfil do mesmo Escopo (Corretora inalterada).</summary>
    public void Reassign(Guid newProfileId) => ProfileId = newProfileId;
}

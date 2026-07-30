using SmartInsure.Core.Entities;

namespace SmartInsure.Core.Abstractions.Repositories;

public interface IUserBrokerageMembershipRepository : IRepository<UserBrokerageMembership>
{
    /// <summary>RN-064: vínculos de Corretora de um Usuário.</summary>
    Task<IReadOnlyCollection<UserBrokerageMembership>> GetByUserAsync(
        Guid userId, CancellationToken cancellationToken);

    /// <summary>RN-064: existe vínculo do Usuário com a Corretora? (par único).</summary>
    Task<bool> ExistsAsync(Guid userId, Guid brokerageId, CancellationToken cancellationToken);

    /// <summary>
    /// RN-064: o vínculo do Usuário naquela Corretora — portador do Perfil que ele tem ali.
    /// É por ele que se decide o que o solicitante pode fazer no Escopo ativo (RN-068/RN-069).
    /// </summary>
    Task<UserBrokerageMembership?> GetByUserAndBrokerageAsync(
        Guid userId,
        Guid brokerageId,
        CancellationToken cancellationToken);
}

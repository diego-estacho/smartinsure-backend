using SmartInsure.Core.Entities;

namespace SmartInsure.Core.Abstractions.Repositories;

public interface IUserBrokerageMembershipRepository : IRepository<UserBrokerageMembership>
{
    /// <summary>RN-034: vínculos de Corretora de um Usuário.</summary>
    Task<IReadOnlyCollection<UserBrokerageMembership>> GetByUserAsync(
        Guid userId, CancellationToken cancellationToken);

    /// <summary>RN-034: existe vínculo do Usuário com a Corretora? (par único).</summary>
    Task<bool> ExistsAsync(Guid userId, Guid brokerageId, CancellationToken cancellationToken);
}

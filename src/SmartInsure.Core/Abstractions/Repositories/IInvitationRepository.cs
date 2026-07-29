using SmartInsure.Core.Entities;

namespace SmartInsure.Core.Abstractions.Repositories;

public interface IInvitationRepository : IRepository<Invitation>
{
    /// <summary>RN-065: obtém convite ativo (não consumido) de um Usuário.</summary>
    Task<Invitation?> GetPendingByUserAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>RN-065: busca convite pelo hash do token.</summary>
    Task<Invitation?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken);
}

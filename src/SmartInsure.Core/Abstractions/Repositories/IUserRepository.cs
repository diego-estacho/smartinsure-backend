using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;

namespace SmartInsure.Core.Abstractions.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken);

    Task<User?> GetByExternalIdentityAsync(string externalIdentity, CancellationToken cancellationToken);

    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken);

    /// <summary>RN-012: a plataforma nunca fica sem Administrador do Sistema.</summary>
    Task<int> CountByProfileAsync(EUserProfile profile, CancellationToken cancellationToken);
}

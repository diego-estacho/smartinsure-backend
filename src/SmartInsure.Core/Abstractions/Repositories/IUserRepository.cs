using SmartInsure.Core.Entities;

namespace SmartInsure.Core.Abstractions.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken);

    Task<User?> GetByExternalIdentityAsync(string externalIdentity, CancellationToken cancellationToken);

    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken);
}

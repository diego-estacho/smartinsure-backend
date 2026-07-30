using SmartInsure.Core.Abstractions.Repositories.Dtos;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;

namespace SmartInsure.Core.Abstractions.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken);

    /// <summary>
    /// Listagem paginada de Usuários com busca por nome/e-mail e filtro de situação. O Escopo
    /// restringe quem aparece (RN-064): informado, devolve apenas Usuários com Vínculo naquela
    /// Corretora ou Tomador; nulo, devolve todos (visão do Administrador do Sistema).
    /// </summary>
    Task<(IReadOnlyList<UserListItemDto> Items, long TotalCount)> ListAsync(
        int page,
        int pageSize,
        string? search,
        EUserStatus? status,
        Guid? brokerageId,
        Guid? policyHolderId,
        CancellationToken cancellationToken);

    /// <summary>Detalhe do Usuário com o Perfil de Escopo System (RN-012) e os Vínculos (RN-064).</summary>
    Task<UserDetailsDto?> GetDetailsByIdAsync(Guid userId, CancellationToken cancellationToken);

    Task<User?> GetByExternalIdentityAsync(string externalIdentity, CancellationToken cancellationToken);

    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken);

    /// <summary>RN-012: a plataforma nunca fica sem Administrador do Sistema.</summary>
    Task<int> CountByProfileIdAsync(Guid profileId, CancellationToken cancellationToken);
}

using SmartInsure.Core.Abstractions.Repositories.Dtos;
using SmartInsure.Core.Entities;

namespace SmartInsure.Core.Abstractions.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken);

    /// <summary>
    /// Listagem paginada de Usuários com busca por nome/e-mail/perfil/vínculo e filtro de situação
    /// (vocabulário da tela, incl. "Expirado" derivado do Convite — RN-065). O Escopo restringe quem
    /// aparece (RN-064): informado, devolve apenas Usuários com Vínculo naquela Corretora ou Tomador;
    /// nulo, devolve todos (visão do Administrador do Sistema). As contagens por situação respeitam
    /// escopo e busca, mas não o próprio filtro de situação (alimentam as abas).
    /// </summary>
    Task<(IReadOnlyList<UserListItemDto> Items, long TotalCount, UserStatusCountsDto Counts)> ListAsync(
        int page,
        int pageSize,
        UserListFilters filters,
        CancellationToken cancellationToken);

    /// <summary>Detalhe do Usuário com o Perfil de Escopo System (RN-012) e os Vínculos (RN-064).</summary>
    Task<UserDetailsDto?> GetDetailsByIdAsync(Guid userId, CancellationToken cancellationToken);

    Task<User?> GetByExternalIdentityAsync(string externalIdentity, CancellationToken cancellationToken);

    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken);

    /// <summary>RN-012: a plataforma nunca fica sem Administrador do Sistema.</summary>
    Task<int> CountByProfileIdAsync(Guid profileId, CancellationToken cancellationToken);
}

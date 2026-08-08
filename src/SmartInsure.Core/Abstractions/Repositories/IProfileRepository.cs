using SmartInsure.Core.Abstractions.Repositories.Dtos;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;

namespace SmartInsure.Core.Abstractions.Repositories;

public interface IProfileRepository : IRepository<Profile>
{
    /// <summary>Catálogo paginado de Perfis, com filtro opcional por Escopo (RN-062).</summary>
    Task<(IReadOnlyList<ProfileListItemDto> Items, long TotalCount)> ListAsync(
        int page,
        int pageSize,
        string? search,
        EProfileScope? scope,
        CancellationToken cancellationToken);

    /// <summary>Detalhe do Perfil com as Permissões marcadas (RN-062/RN-063).</summary>
    Task<ProfileDetailsDto?> GetDetailsByIdAsync(Guid profileId, CancellationToken cancellationToken);

    /// <summary>
    /// RN-072: Perfis de um Escopo aplicáveis a um dono — os globais (sem Corretora/Tomador) mais
    /// os customizados daquele dono. Perfil customizado de outra Corretora/Tomador nunca aparece.
    /// </summary>
    Task<IReadOnlyList<Profile>> ListByScopeAsync(
        EProfileScope scope,
        Guid? ownerId,
        CancellationToken cancellationToken);

    /// <summary>
    /// RN-069/RN-070: já existe Perfil com este nome no mesmo Escopo? (nome repetido dentro da
    /// mesma Corretora/Tomador é recusado). `excludedProfileId` ignora o próprio na edição.
    /// </summary>
    Task<bool> ExistsByNameInScopeAsync(
        string name,
        EProfileScope scope,
        Guid? ownerId,
        Guid? excludedProfileId,
        CancellationToken cancellationToken);

    /// <summary>Perfil rastreado com as Permissões carregadas, para edição (RN-073/RN-074).</summary>
    Task<Profile?> GetTrackedByIdAsync(Guid profileId, CancellationToken cancellationToken);

    /// <summary>
    /// RN-074: remove o Perfil e seus ProfilePermissions (FKs Restrict, sem cascade — os filhos vão
    /// antes do pai). Não persiste: o commit fica com o caso de uso.
    /// </summary>
    void RemoveWithPermissions(Profile profile);

    /// <summary>
    /// RN-074: quantos Usuários usam este Perfil — soma Vínculos de Corretora, de Tomador e o
    /// Perfil de Escopo Sistema. Remoção é recusada enquanto houver Usuário.
    /// </summary>
    Task<int> CountUsersByProfileAsync(Guid profileId, CancellationToken cancellationToken);

    /// <summary>
    /// RN-074: uso de um conjunto de Perfis para a listagem — nº de Usuários e nº de Áreas tocadas
    /// por Perfil, em poucas consultas (evita N+1). Ids ausentes retornam 0/0.
    /// </summary>
    Task<IReadOnlyDictionary<Guid, ProfileUsageDto>> GetUsageAsync(
        IReadOnlyCollection<Guid> profileIds,
        CancellationToken cancellationToken);

    /// <summary>
    /// RN-074/RN-075: migra os Vínculos de Usuário de um Perfil para outro do mesmo Escopo, sem
    /// persistir — deixa as entidades rastreadas para o commit do caso de uso, junto da remoção.
    /// </summary>
    Task ReassignMembershipsAsync(
        Guid fromProfileId,
        Guid toProfileId,
        EProfileScope scope,
        CancellationToken cancellationToken);

    /// <summary>Perfil pela chave natural (nome) — usado para resolver o Perfil a conceder (RN-012).</summary>
    Task<Profile?> GetByNameAsync(string name, CancellationToken cancellationToken);

    /// <summary>Perfil fixo Administrador do Sistema (Escopo System) — chave natural, nunca o GUID (RN-012).</summary>
    Task<Profile?> GetSystemAdministratorAsync(CancellationToken cancellationToken);

    /// <summary>Perfil fixo Corretor Administrador (Escopo Brokerage, global) — chave natural (RN-066).</summary>
    Task<Profile?> GetBrokerageAdministratorAsync(CancellationToken cancellationToken);
}

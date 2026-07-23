using SmartInsure.Core.Abstractions.Repositories.Dtos;
using SmartInsure.Core.Entities;

namespace SmartInsure.Core.Abstractions.Repositories;

public interface IInsurerRepository : IRepository<Insurer>
{
    /// <summary>RN-007/RN-008: CNPJ único no catálogo (exceção opcional para a própria seguradora na alteração).</summary>
    Task<bool> CnpjExistsAsync(string cnpj, Guid? exceptInsurerId, CancellationToken cancellationToken);

    /// <summary>RN-010: consulta padrão só Ativas; visão completa quando includeInactive.</summary>
    Task<(IReadOnlyList<InsurerListItemDto> Items, long TotalCount)> ListAsync(
        int page, int pageSize, bool includeInactive, CancellationToken cancellationToken);

    /// <summary>RN-027: Seguradora rastreada para validação de status.</summary>
    Task<Insurer?> GetTrackedByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>RN-031: batch load de nomes corporativos para evitar N+1 em listagens.</summary>
    Task<IReadOnlyDictionary<Guid, string>> GetCorporateNamesByIdsAsync(
        IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken);
}

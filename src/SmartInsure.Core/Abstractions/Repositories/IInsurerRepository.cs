using SmartInsure.Core.Abstractions.Repositories.Dtos;
using SmartInsure.Core.Entities;

namespace SmartInsure.Core.Abstractions.Repositories;

public interface IInsurerRepository : IRepository<Insurer>
{
    /// <summary>RN-005/RN-006: CNPJ único no catálogo (exceção opcional para a própria seguradora na alteração).</summary>
    Task<bool> CnpjExistsAsync(string cnpj, Guid? exceptInsurerId, CancellationToken cancellationToken);

    /// <summary>RN-008: consulta padrão só Ativas; visão completa quando includeInactive.</summary>
    Task<(IReadOnlyList<InsurerListItemDto> Items, long TotalCount)> ListAsync(
        int page, int pageSize, bool includeInactive, CancellationToken cancellationToken);
}

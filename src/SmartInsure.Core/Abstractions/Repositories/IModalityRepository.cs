using SmartInsure.Core.Abstractions.Repositories.Dtos;
using SmartInsure.Core.Entities;

namespace SmartInsure.Core.Abstractions.Repositories;

public interface IModalityRepository : IRepository<Modality>
{
    /// <summary>RN-029: nome da Modalidade único no catálogo (exceção opcional para a própria Modalidade na edição).</summary>
    Task<bool> NameExistsAsync(string name, Guid? exceptModalityId, CancellationToken cancellationToken);

    /// <summary>RN-036: consulta padrão só Ativas; visão completa quando includeInactive.</summary>
    Task<(IReadOnlyList<ModalityListItemDto> Items, long TotalCount)> ListAsync(
        int page, int pageSize, bool includeInactive, CancellationToken cancellationToken);

    /// <summary>RN-033: todas as Modalidades Ativas (sem paginação) para montar o Mapa.</summary>
    Task<IReadOnlyList<ModalityListItemDto>> ListActiveForMapAsync(CancellationToken cancellationToken);
}

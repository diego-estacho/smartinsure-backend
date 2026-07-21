using SmartInsure.Core.Abstractions.Repositories.Dtos;
using SmartInsure.Core.Entities;

namespace SmartInsure.Core.Abstractions.Repositories;

public interface IModalityGroupRepository : IRepository<ModalityGroup>
{
    /// <summary>RN-029: nome do Grupo único no catálogo (exceção opcional para o próprio Grupo na edição).</summary>
    Task<bool> NameExistsAsync(string name, Guid? exceptGroupId, CancellationToken cancellationToken);

    /// <summary>RN-036: consulta padrão só Ativos; visão completa quando includeInactive.</summary>
    Task<(IReadOnlyList<ModalityGroupListItemDto> Items, long TotalCount)> ListAsync(
        int page, int pageSize, bool includeInactive, CancellationToken cancellationToken);
}

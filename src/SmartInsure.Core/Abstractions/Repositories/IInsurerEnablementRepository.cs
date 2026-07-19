using SmartInsure.Core.Abstractions.Repositories.Dtos;
using SmartInsure.Core.Entities;

namespace SmartInsure.Core.Abstractions.Repositories;

public interface IInsurerEnablementRepository : IRepository<InsurerEnablement>
{
    /// <summary>RN-022: no máximo uma Habilitação por par Corretora×Seguradora.</summary>
    Task<bool> PairExistsAsync(Guid brokerageId, Guid insurerId, CancellationToken cancellationToken);

    /// <summary>RN-023: Habilitação do par para resolução do Motor de Cálculo (rastreada não — leitura).</summary>
    Task<InsurerEnablement?> GetByPairAsync(Guid brokerageId, Guid insurerId, CancellationToken cancellationToken);

    Task<InsurerEnablementDetailsDto?> GetDetailsByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<(IReadOnlyList<InsurerEnablementListItemDto> Items, long TotalCount)> ListAsync(
        Guid? brokerageId, int page, int pageSize, CancellationToken cancellationToken);
}

using SmartInsure.Core.Abstractions.Repositories.Dtos;
using SmartInsure.Core.Entities;

namespace SmartInsure.Core.Abstractions.Repositories;

public interface IBrokerageInsurerEnablementRepository : IRepository<BrokerageInsurerEnablement>
{
    /// <summary>RN-022: no máximo uma Habilitação por par Corretora×Seguradora.</summary>
    Task<bool> PairExistsAsync(Guid brokerageId, Guid insurerId, CancellationToken cancellationToken);

    /// <summary>RN-023: Habilitação do par para resolução do Motor de Cálculo (rastreada não — leitura).</summary>
    Task<BrokerageInsurerEnablement?> GetByPairAsync(Guid brokerageId, Guid insurerId, CancellationToken cancellationToken);

    Task<BrokerageInsurerEnablementDetailsDto?> GetDetailsByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<(IReadOnlyList<BrokerageInsurerEnablementListItemDto> Items, long TotalCount)> ListAsync(
        Guid? brokerageId, int page, int pageSize, CancellationToken cancellationToken);

    /// <summary>RN-031: Habilitações Ativas com CNPJ da Corretora e Referência de origem da Seguradora, para a importação.</summary>
    Task<IReadOnlyList<ActiveEnablementImportDto>> ListActiveForImportAsync(CancellationToken cancellationToken);
}

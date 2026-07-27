using SmartInsure.Core.Entities;

namespace SmartInsure.Core.Abstractions.Repositories;

public interface IQuotationGroupRepository : IRepository<QuotationGroup>
{
    /// <summary>RN-051: Grupo de Cotação rastreado com as Seguradoras do escopo, para atualizar no lugar.</summary>
    Task<QuotationGroup?> GetByIdWithInsurersAsync(Guid id, CancellationToken cancellationToken);
}

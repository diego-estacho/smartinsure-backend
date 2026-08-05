using SmartInsure.Core.Entities;

namespace SmartInsure.Core.Abstractions.Repositories;

public interface IQuotationGroupRepository : IRepository<QuotationGroup>
{
    /// <summary>RN-051: Grupo de Cotação rastreado com as Seguradoras do escopo, para atualizar no lugar.</summary>
    Task<QuotationGroup?> GetByIdWithInsurersAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// RN-503: o Grupo com a réplica do endereço do Segurado carregada. A emissão precisa dela — sem o
    /// carregamento explícito a navegação vem nula e o portão reprovaria uma oferta que TEM endereço
    /// (defeito encontrado no E2E de emissão).
    /// </summary>
    Task<QuotationGroup?> GetByIdWithInsuredAddressAsync(Guid id, CancellationToken cancellationToken);
}

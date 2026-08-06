using SmartInsure.Core.Entities;

namespace SmartInsure.Core.Abstractions.Repositories;

public interface IQuotationGroupRepository : IRepository<QuotationGroup>
{
    /// <summary>RN-051: Grupo de Cotação rastreado com as Seguradoras do escopo, para atualizar no lugar.</summary>
    Task<QuotationGroup?> GetByIdWithInsurersAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// RN-104/RN-105: ids das Coberturas Adicionais canônicas escolhidas no Grupo. É consulta PROJETADA
    /// de propósito, não navegação: o fan-out carrega o Grupo por id (FindAsync, sem Include), então
    /// depender da coleção carregada faria a resolução receber lista vazia em silêncio — a cobertura
    /// deixaria de ser enviada sem erro nenhum, que é exatamente o defeito que RN-105 corrige.
    /// </summary>
    Task<IReadOnlyList<Guid>> ListAdditionalCoverageIdsAsync(
        Guid quotationGroupId, CancellationToken cancellationToken);

    /// <summary>
    /// RN-503: o Grupo com a réplica do endereço do Segurado carregada. A emissão precisa dela — sem o
    /// carregamento explícito a navegação vem nula e o portão reprovaria uma oferta que TEM endereço
    /// (defeito encontrado no E2E de emissão).
    /// </summary>
    Task<QuotationGroup?> GetByIdWithInsuredAddressAsync(Guid id, CancellationToken cancellationToken);
}

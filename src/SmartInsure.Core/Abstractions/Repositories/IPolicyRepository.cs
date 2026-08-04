using SmartInsure.Core.Entities;

namespace SmartInsure.Core.Abstractions.Repositories;

/// <summary>
/// Repositório da Apólice (RN-507/RN-514). A conclusão da unidade de trabalho é do UseCase via
/// <see cref="IUnitOfWork"/> (ADR-036).
/// </summary>
public interface IPolicyRepository : IRepository<Policy>
{
    /// <summary>
    /// RN-507: já existe Apólice para esta Cotação? Cada Cotação admite uma única solicitação de
    /// emissão — a segunda é recusada aqui, sem acionar a Seguradora.
    /// </summary>
    Task<bool> ExistsForQuotationAsync(Guid quotationId, CancellationToken cancellationToken);
}

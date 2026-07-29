using SmartInsure.Core.Entities;

namespace SmartInsure.Core.Abstractions.Repositories;

/// <summary>
/// Repositório da Cotação (RN-057..063). A conclusão da unidade de trabalho é sempre do UseCase
/// via <see cref="IUnitOfWork"/> (ADR-036).
/// </summary>
public interface IQuotationRepository : IRepository<Quotation>
{
    /// <summary>RN-057: Cotações do Grupo, para o acompanhamento (polling) e a seleção.</summary>
    Task<IReadOnlyList<Quotation>> ListByGroupAsync(Guid quotationGroupId, CancellationToken cancellationToken);

    /// <summary>RN-056/RN-057: persiste em lote as Cotações materializadas pelo fan-out.</summary>
    Task AddRangeAsync(IEnumerable<Quotation> quotations, CancellationToken cancellationToken);
}

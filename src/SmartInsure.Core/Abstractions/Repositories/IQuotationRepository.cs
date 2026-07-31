using SmartInsure.Core.Abstractions.Repositories.Dtos;
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

    /// <summary>
    /// ADR-050: Cotações Requested cujo lease expirou — sem processamento iniciado (ou iniciado) antes de
    /// <paramref name="staleBeforeUtc"/>. São as órfãs que o reconciliador reenfileira após restart, sem
    /// tocar as que ainda estão em voo. Traz o work item pronto (com a Corretora do Grupo).
    /// </summary>
    Task<IReadOnlyList<QuotationRequestWorkItem>> ListStaleRequestedWorkItemsAsync(
        DateTime staleBeforeUtc, CancellationToken cancellationToken);

    /// <summary>
    /// RN-077/RN-078: o "livro" de Cotações da Corretora — projeção achatada (Grupo/Tomador/Segurado/
    /// Modalidade), paginada e filtrada, com a contagem por situação. Inclui só as **obtidas com
    /// resultado do provedor** (exclui Requested/Failed e indisponibilidades de origem local); escopo
    /// pela Corretora do Grupo (RN-064). A contagem respeita a busca mas ignora a situação filtrada.
    /// </summary>
    Task<QuotationBookPageDto> ListBookAsync(QuotationBookFilter filter, CancellationToken cancellationToken);
}

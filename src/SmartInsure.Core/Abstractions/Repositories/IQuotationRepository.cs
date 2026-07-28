using SmartInsure.Core.Entities;

namespace SmartInsure.Core.Abstractions.Repositories;

/// <summary>Repositório de Cotações (RN-057..061). Agregado próprio, persistido por Seguradora.</summary>
public interface IQuotationRepository
{
    Task AddRangeAsync(IEnumerable<Quotation> quotations, CancellationToken cancellationToken);

    /// <summary>Carrega uma Cotação rastreada (para o consumidor gravar o resultado).</summary>
    Task<Quotation?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Cotações de um Grupo, com motivos (leitura para a tela — AsNoTracking).</summary>
    Task<IReadOnlyList<Quotation>> ListByGroupAsync(Guid quotationGroupId, CancellationToken cancellationToken);

    /// <summary>Remove as Cotações de um Grupo (invalidação/recálculo — RN-060).</summary>
    Task RemoveByGroupAsync(Guid quotationGroupId, CancellationToken cancellationToken);

    /// <summary>RN-057/ADR-050: Cotações ainda em Requested há mais que a janela (reconciliador reenfileira).</summary>
    Task<IReadOnlyList<Quotation>> ListStaleRequestedAsync(DateTime olderThanUtc, CancellationToken cancellationToken);
}

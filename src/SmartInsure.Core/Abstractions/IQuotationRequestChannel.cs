using SmartInsure.Core.Abstractions.Channels;

namespace SmartInsure.Core.Abstractions;

/// <summary>
/// Item de trabalho do fan-out de cotação (ADR-050): identifica UMA Cotação a obter de UMA Seguradora.
/// Só IDs — os dados de risco/conexão são carregados pelo consumidor (BackgroundService) em escopo próprio.
/// </summary>
public sealed record QuotationRequestWorkItem(
    Guid QuotationId,
    Guid QuotationGroupId,
    Guid InsurerId,
    Guid BrokerageId);

/// <summary>
/// Canal dedicado do fan-out de cotação (ADR-050): a solicitação enfileira itens e o consumidor os
/// obtém e persiste incrementalmente (RN-057). Herda o contrato base de fila in-process.
/// </summary>
public interface IQuotationRequestChannel : IWorkItemChannel<QuotationRequestWorkItem>;

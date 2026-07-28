namespace SmartInsure.Core.Abstractions.Channels;

/// <summary>
/// Contrato dedicado da fila de cotação (ADR-050): fan-out das Cotações às Seguradoras.
/// Herda o contrato base para permitir uma implementação/capacidade próprias.
/// </summary>
public interface IQuotationRequestChannel : IWorkItemChannel<QuotationRequestWorkItem>
{
}

namespace SmartInsure.Core.Abstractions;

/// <summary>
/// Processa UM item do fan-out de cotação (RN-057): obtém a Cotação de uma Seguradora e persiste o
/// resultado. Implementado na camada de aplicação; resolvido em escopo próprio pelo consumidor (ADR-050).
/// </summary>
public interface IQuotationRequestProcessor
{
    Task ProcessAsync(QuotationRequestWorkItem workItem, CancellationToken cancellationToken);
}

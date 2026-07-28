namespace SmartInsure.Core.Abstractions.Channels;

/// <summary>
/// Work item do fan-out de cotação (RN-057, ADR-050): uma Cotação a obter, por (Grupo, Seguradora).
/// A Cotação já está persistida em Requested antes de o item ser enfileirado — a fila é otimização
/// de latência, o banco é o registro.
/// </summary>
public sealed record QuotationRequestWorkItem(Guid QuotationId, Guid QuotationGroupId, Guid InsurerId);

using SmartInsure.Core.Abstractions;

namespace SmartInsure.Infra.BackgroundServices.Channels;

/// <summary>
/// Fila in-process do fan-out de cotação (ADR-050): bounded com backpressure. Capacidade fixa
/// provisória (candidata a Options/appsettings quando a escala pedir).
/// </summary>
public sealed class QuotationRequestChannel()
    : BoundedWorkItemChannel<QuotationRequestWorkItem>(1000), IQuotationRequestChannel;

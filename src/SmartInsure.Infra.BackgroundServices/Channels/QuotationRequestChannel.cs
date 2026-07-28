using Microsoft.Extensions.Options;
using SmartInsure.Core.Abstractions.Channels;
using SmartInsure.Infra.BackgroundServices.Options;

namespace SmartInsure.Infra.BackgroundServices.Channels;

/// <summary>Fila in-process do fan-out de cotação (ADR-050), com capacidade configurável.</summary>
public sealed class QuotationRequestChannel(IOptions<QuotationFanOutOptions> options)
    : BoundedWorkItemChannel<QuotationRequestWorkItem>(options.Value.ChannelCapacity), IQuotationRequestChannel;

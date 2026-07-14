using System.Threading.Channels;
using SmartInsure.Core.Abstractions.Channels;

namespace SmartInsure.Infra.BackgroundServices.Channels;

/// <summary>
/// Implementação base de fila in-process (ADR-050): Channel bounded com backpressure
/// (produtor aguarda quando cheio). Cada operação assíncrona deriva sua implementação
/// concreta com a capacidade adequada.
/// </summary>
public abstract class BoundedWorkItemChannel<TWorkItem> : IWorkItemChannel<TWorkItem>
{
    private readonly Channel<TWorkItem> _channel;

    protected BoundedWorkItemChannel(int capacity)
    {
        _channel = Channel.CreateBounded<TWorkItem>(new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
        });
    }

    public ValueTask EnqueueAsync(TWorkItem workItem, CancellationToken cancellationToken)
        => _channel.Writer.WriteAsync(workItem, cancellationToken);

    public IAsyncEnumerable<TWorkItem> DequeueAllAsync(CancellationToken cancellationToken)
        => _channel.Reader.ReadAllAsync(cancellationToken);
}

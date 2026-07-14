namespace SmartInsure.Core.Abstractions.Channels;

/// <summary>
/// Contrato base de fila in-process (ADR-050). Cada operação assíncrona define seu
/// contrato dedicado (I{Operação}Channel) herdando deste; o estado da operação é
/// persistido antes do enfileiramento — a fila nunca é o único registro do trabalho.
/// </summary>
public interface IWorkItemChannel<TWorkItem>
{
    ValueTask EnqueueAsync(TWorkItem workItem, CancellationToken cancellationToken);

    IAsyncEnumerable<TWorkItem> DequeueAllAsync(CancellationToken cancellationToken);
}

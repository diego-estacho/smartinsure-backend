namespace SmartInsure.Application.UseCase.ModelsBase;

/// <summary>Envelope padrão de listagem paginada (ADR-012).</summary>
public sealed record PagedResponse<TItem>(
    IReadOnlyList<TItem> Items,
    int Page,
    int PageSize,
    long TotalCount)
{
    public long TotalPages => PageSize > 0
        ? (long)Math.Ceiling(TotalCount / (double)PageSize)
        : 0;
}

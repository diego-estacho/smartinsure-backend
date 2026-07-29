namespace SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.ListBrokerages.Responses;

/// <summary>
/// RN-018 — envelope da listagem de Corretoras: página + contagem por situação apresentada
/// (as abas Todas/Ativas/Incompletas/Inativas), considerando os demais filtros aplicados.
/// </summary>
public sealed record ListBrokeragesResponse(
    IReadOnlyList<BrokerageListItemResponse> Items,
    int Page,
    int PageSize,
    long TotalCount,
    BrokerageSituationCountsResponse Counts)
{
    public long TotalPages => PageSize > 0
        ? (long)Math.Ceiling(TotalCount / (double)PageSize)
        : 0;
}

public sealed record BrokerageSituationCountsResponse(
    long All,
    long Active,
    long Incomplete,
    long Inactive);

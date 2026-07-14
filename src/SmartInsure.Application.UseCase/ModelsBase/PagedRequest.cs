namespace SmartInsure.Application.UseCase.ModelsBase;

/// <summary>Base de request de listagem paginada (ADR-012).</summary>
public record PagedRequest
{
    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 20;
}

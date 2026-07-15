namespace SmartInsure.Integration.Casdoor.Models;

public sealed record CasdoorResponse<TData>
{
    public string Status { get; init; } = string.Empty;

    public string? Msg { get; init; }

    public TData? Data { get; init; }

    public bool IsOk => string.Equals(Status, "ok", StringComparison.OrdinalIgnoreCase);
}

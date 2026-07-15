namespace SmartInsure.Integration.Casdoor.Models;

public sealed record CasdoorUser
{
    public string Owner { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string? Id { get; init; }

    public string DisplayName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string? Password { get; init; }

    public string? SignupApplication { get; init; }

    /// <summary>Troca de senha obrigatória no próximo login (RN-001/RN-002).</summary>
    public bool NeedUpdatePassword { get; init; }
}

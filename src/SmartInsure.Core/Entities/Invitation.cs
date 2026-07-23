using System.Security.Cryptography;
using System.Text;

namespace SmartInsure.Core.Entities;

/// <summary>RN-035: convite de primeiro acesso com token de uso único e validade.</summary>
public sealed class Invitation : EntityBase
{
    private Invitation()
    {
    }

    public Guid UserId { get; private set; }

    public string TokenHash { get; private set; } = null!;

    public DateTime ExpiresAtUtc { get; private set; }

    public DateTime? ConsumedAtUtc { get; private set; }

    /// <summary>RN-035: cria convite com token aleatório forte. Retorna (entidade, token plaintext).</summary>
    public static (Invitation invitation, string plainToken) Create(Guid userId, int expiryDays)
    {
        var plainToken = GenerateSecureToken();
        var tokenHash = HashToken(plainToken);
        var expiresAtUtc = DateTime.UtcNow.AddDays(expiryDays);

        var invitation = new Invitation
        {
            UserId = userId,
            TokenHash = tokenHash,
            ExpiresAtUtc = expiresAtUtc,
        };

        return (invitation, plainToken);
    }

    /// <summary>RN-035: marca o convite como consumido (primeiro acesso realizado).</summary>
    public void Consume()
    {
        ConsumedAtUtc = DateTime.UtcNow;
    }

    /// <summary>RN-035: indica se o convite ainda é válido (não consumido e não expirado).</summary>
    public bool IsValid() => ConsumedAtUtc == null && DateTime.UtcNow < ExpiresAtUtc;

    private static string GenerateSecureToken()
    {
        using var rng = RandomNumberGenerator.Create();
        var tokenData = new byte[32];
        rng.GetBytes(tokenData);
        return Convert.ToBase64String(tokenData);
    }

    private static string HashToken(string plainToken)
    {
        using var sha256 = SHA256.Create();
        var hashData = sha256.ComputeHash(Encoding.UTF8.GetBytes(plainToken));
        return Convert.ToHexString(hashData);
    }
}

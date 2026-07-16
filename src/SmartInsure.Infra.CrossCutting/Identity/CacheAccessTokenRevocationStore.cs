using Microsoft.Extensions.Caching.Distributed;
using SmartInsure.Core.Abstractions.Services;

namespace SmartInsure.Infra.CrossCutting.Identity;

/// <summary>
/// Denylist de acessos encerrados (RN-006) sobre IDistributedCache (ADR-040): a entrada
/// vive até a expiração natural do token — depois disso a recusa já decorre do lifetime.
/// </summary>
public sealed class CacheAccessTokenRevocationStore(IDistributedCache cache)
    : IAccessTokenRevocationStore
{
    private static string Key(string tokenId) => $"revoked-access-token:{tokenId}";

    public async Task RevokeAsync(
        string tokenId, DateTime expiresAtUtc, CancellationToken cancellationToken)
    {
        if (expiresAtUtc <= DateTime.UtcNow)
        {
            return;
        }

        await cache.SetStringAsync(
            Key(tokenId),
            "revoked",
            new DistributedCacheEntryOptions { AbsoluteExpiration = expiresAtUtc },
            cancellationToken);
    }

    public async Task<bool> IsRevokedAsync(string tokenId, CancellationToken cancellationToken)
        => await cache.GetStringAsync(Key(tokenId), cancellationToken) is not null;
}

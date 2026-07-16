using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Caching.Distributed;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Constants;
using SmartInsure.Core.Enumerators;

namespace SmartInsure.Api.Services;

/// <summary>
/// Enriquecimento de claims com o Perfil do Usuário (ADR-014, RN-009/RN-010): perfil lido
/// da plataforma (com cache curto, invalidado na concessão/revogação) e exposto como role.
/// </summary>
public sealed class UserProfileClaimsTransformation(
    IUserRepository userRepository,
    IDistributedCache cache) : IClaimsTransformation
{
    private static readonly DistributedCacheEntryOptions CacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
    };

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        var externalIdentity = principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? principal.FindFirstValue("sub");

        if (string.IsNullOrEmpty(externalIdentity)
            || principal.IsInRole(Roles.SystemAdministrator))
        {
            return principal;
        }

        var cacheKey = CacheKeys.UserProfile(externalIdentity);
        var profile = await cache.GetStringAsync(cacheKey);

        if (profile is null)
        {
            var user = await userRepository.GetByExternalIdentityAsync(
                externalIdentity, CancellationToken.None);
            profile = user?.Profile?.ToString() ?? string.Empty;
            await cache.SetStringAsync(cacheKey, profile, CacheOptions);
        }

        if (profile == EUserProfile.SystemAdministrator.ToString())
        {
            var identity = new ClaimsIdentity();
            identity.AddClaim(new Claim(ClaimTypes.Role, Roles.SystemAdministrator));
            principal.AddIdentity(identity);
        }

        return principal;
    }
}

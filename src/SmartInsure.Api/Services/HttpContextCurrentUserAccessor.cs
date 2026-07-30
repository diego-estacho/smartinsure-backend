using System.Security.Claims;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Constants;

namespace SmartInsure.Api.Services;

/// <summary>
/// Identidade corrente a partir das claims enriquecidas (ADR-014) — serviço exclusivo
/// da borda HTTP; execuções de sistema (Functions) não registram este accessor.
/// </summary>
public sealed class HttpContextCurrentUserAccessor(IHttpContextAccessor httpContextAccessor)
    : ICurrentUserAccessor
{
    public string? UserIdentifier
    {
        get
        {
            var user = httpContextAccessor.HttpContext?.User;

            return user?.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? user?.FindFirstValue("sub");
        }
    }

    public string? SessionTokenId
        => httpContextAccessor.HttpContext?.User.FindFirstValue("jti");

    public DateTime? SessionExpiresAtUtc
    {
        get
        {
            var exp = httpContextAccessor.HttpContext?.User.FindFirstValue("exp");

            return long.TryParse(exp, out var unixSeconds)
                ? DateTimeOffset.FromUnixTimeSeconds(unixSeconds).UtcDateTime
                : null;
        }
    }

    /// <summary>RN-064/ADR-065: Escopo ativo lido do próprio acesso, nunca do corpo da requisição.</summary>
    public Guid? ActiveBrokerageId => ReadScope(ScopeClaimNames.ActiveBrokerage);

    public Guid? ActivePolicyHolderId => ReadScope(ScopeClaimNames.ActivePolicyHolder);

    private Guid? ReadScope(string claimName)
    {
        var value = httpContextAccessor.HttpContext?.User.FindFirstValue(claimName);

        return Guid.TryParse(value, out var scopeId) ? scopeId : null;
    }
}

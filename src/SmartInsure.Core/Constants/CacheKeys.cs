namespace SmartInsure.Core.Constants;

/// <summary>Chaves de cache distribuído (ADR-040).</summary>
public static class CacheKeys
{
    /// <summary>Perfil por identidade externa; invalidada na concessão/revogação (RN-012).</summary>
    public static string UserProfile(string externalIdentity) => $"user-profile:{externalIdentity}";
}

namespace SmartInsure.Core.Abstractions.Services;

/// <summary>
/// Accessor de identidade corrente (ADR-014, ADR-035). Registrado apenas na borda HTTP;
/// resolução nula significa execução de sistema (Functions/jobs), auditada como usuário-sistema.
/// </summary>
public interface ICurrentUserAccessor
{
    string? UserIdentifier { get; }

    /// <summary>Identificador único do acesso autenticado corrente (claim jti) — RN-006.</summary>
    string? SessionTokenId { get; }

    /// <summary>Expiração do acesso autenticado corrente (claim exp), em UTC — RN-006.</summary>
    DateTime? SessionExpiresAtUtc { get; }
}

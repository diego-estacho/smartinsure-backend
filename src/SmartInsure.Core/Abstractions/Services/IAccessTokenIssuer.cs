using SmartInsure.Core.Entities;

namespace SmartInsure.Core.Abstractions.Services;

/// <summary>
/// Emissor do acesso autenticado da plataforma (RN-005): concedido após validação das
/// credenciais no provedor de identidade, com validade de 8 horas.
/// </summary>
public interface IAccessTokenIssuer
{
    AccessToken IssueFor(User user);
}

/// <summary>Acesso autenticado emitido pela plataforma (RN-005).</summary>
/// <param name="Token">Token de acesso.</param>
/// <param name="ExpiresAtUtc">Fim da validade, em UTC.</param>
public sealed record AccessToken(string Token, DateTime ExpiresAtUtc);

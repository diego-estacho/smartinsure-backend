using SmartInsure.Core.Entities;

namespace SmartInsure.Core.Abstractions.Services;

/// <summary>
/// Emissor do acesso autenticado da plataforma (RN-005): concedido após validação das
/// credenciais no provedor de identidade, com validade de 8 horas. O Escopo ativo (RN-064)
/// viaja no próprio acesso (ADR-065) — trocar de Corretora/Tomador reemite o acesso.
/// </summary>
public interface IAccessTokenIssuer
{
    AccessToken IssueFor(User user, ActiveScope activeScope);
}

/// <summary>Acesso autenticado emitido pela plataforma (RN-005).</summary>
/// <param name="Token">Token de acesso.</param>
/// <param name="ExpiresAtUtc">Fim da validade, em UTC.</param>
public sealed record AccessToken(string Token, DateTime ExpiresAtUtc);

/// <summary>
/// Escopo ativo do acesso (RN-064, ADR-065): a Corretora ativa e o Tomador ativo em uso.
/// Nulo significa "sem Escopo daquele tipo em uso" — o Usuário opera apenas Escopo Sistema
/// ou ainda precisa escolher entre os vínculos que tem.
/// </summary>
/// <param name="BrokerageId">Corretora ativa, quando houver.</param>
/// <param name="PolicyHolderId">Tomador ativo, quando houver.</param>
public sealed record ActiveScope(Guid? BrokerageId, Guid? PolicyHolderId)
{
    public static ActiveScope None { get; } = new(null, null);
}

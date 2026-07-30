namespace SmartInsure.Core.Constants;

/// <summary>
/// Claims que transportam o Escopo ativo do acesso (RN-064, ADR-065): a Corretora ativa e o
/// Tomador ativo em uso. Emitidas no login e reemitidas na troca de Escopo — nunca informadas
/// pelo cliente por outro caminho (SECURITY.md: o Escopo é decisão do servidor).
/// </summary>
public static class ScopeClaimNames
{
    public const string ActiveBrokerage = "active_brokerage";

    public const string ActivePolicyHolder = "active_policy_holder";
}

using SmartInsure.Core.Abstractions.Services;

namespace SmartInsure.Application.UseCase.Services.Scopes;

/// <summary>
/// Resolve o Escopo ativo do acesso (RN-064): a Corretora ativa e o Tomador ativo.
/// Compartilhado pelo login (Escopo padrão) e pela troca de Escopo — a validação do vínculo
/// é sempre do servidor, nunca do cliente (SECURITY.md).
/// </summary>
public interface IActiveScopeResolver
{
    /// <summary>
    /// Escopo padrão no primeiro acesso e em cada login (decisão do dono do produto em 2026-07-29,
    /// [OPEN-19]): vínculo único vira ativo automaticamente; com mais de um, nenhum é escolhido
    /// pelo servidor — o Usuário seleciona antes de operar naquele Escopo.
    /// </summary>
    Task<ActiveScope> ResolveDefaultAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Escopo pedido pelo Usuário na troca: cada identificador informado precisa ter vínculo
    /// (RN-064) — sem vínculo, a troca é recusada. Identificador nulo significa sair daquele Escopo.
    /// </summary>
    Task<ActiveScope> ResolveRequestedAsync(
        Guid userId,
        Guid? brokerageId,
        Guid? policyHolderId,
        CancellationToken cancellationToken);
}

using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;

namespace SmartInsure.Application.UseCase.Services.Scopes;

/// <summary>
/// Autorização por Escopo ativo (RN-064): confere que o solicitante é o administrador do Escopo
/// em que está operando. Diferente do Administrador do Sistema — esse é policy de rota (role no
/// acesso); Corretor e Tomador Administrador são Perfis por Vínculo, então a checagem é de dado.
/// A decisão é sempre do servidor (SECURITY.md).
/// </summary>
public interface IScopeAuthorization
{
    /// <summary>
    /// RN-068/RN-069: exige solicitante Ativo, com Corretora ativa selecionada e Perfil
    /// Corretor Administrador nela. Devolve o solicitante e a Corretora ativa conferidos.
    /// </summary>
    Task<ScopeActor> RequireBrokerageAdministratorAsync(
        string externalIdentity,
        Guid? activeBrokerageId,
        CancellationToken cancellationToken);

    /// <summary>
    /// RN-070: exige solicitante Ativo, com Tomador ativo selecionado e Perfil Tomador
    /// Administrador nele.
    /// </summary>
    Task<ScopeActor> RequirePolicyHolderAdministratorAsync(
        string externalIdentity,
        Guid? activePolicyHolderId,
        CancellationToken cancellationToken);

    /// <summary>
    /// RN-069/RN-070: o Escopo que o solicitante administra agora — Corretora ativa, se ele é
    /// Corretor Administrador nela; senão Tomador ativo, se é Tomador Administrador nele.
    /// Usado pelos fluxos que valem nos dois Escopos (ex.: manter Perfis customizados).
    /// </summary>
    Task<AdministeredScope> RequireScopeAdministratorAsync(
        string externalIdentity,
        Guid? activeBrokerageId,
        Guid? activePolicyHolderId,
        CancellationToken cancellationToken);
}

/// <summary>Solicitante conferido e o Escopo ativo em que ele age (RN-064).</summary>
/// <param name="User">Usuário solicitante.</param>
/// <param name="ScopeId">Corretora ativa ou Tomador ativo conferido.</param>
public sealed record ScopeActor(User User, Guid ScopeId);

/// <summary>
/// Escopo administrado pelo solicitante (RN-069/RN-070): qual tipo de Escopo e qual dono.
/// </summary>
/// <param name="User">Usuário solicitante.</param>
/// <param name="Scope">Corretora ou Tomador.</param>
/// <param name="OwnerId">Corretora ativa ou Tomador ativo.</param>
public sealed record AdministeredScope(User User, EProfileScope Scope, Guid OwnerId);

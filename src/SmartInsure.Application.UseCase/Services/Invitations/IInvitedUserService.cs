using SmartInsure.Core.Entities;

namespace SmartInsure.Application.UseCase.Services.Invitations;

/// <summary>
/// Criação de Usuário convidado (RN-065): identidade no provedor, Usuário Pendente, Convite e
/// Vínculos numa transação, com compensação da identidade em caso de falha e e-mail pós-commit.
///
/// Nasceu na fatia dos fluxos de criação por Corretor/Tomador Administrador (RN-068/RN-069/RN-070),
/// que compartilham exatamente esse miolo — os dois fluxos anteriores (`CreateUser` e
/// `InviteBrokerageAdministrator`) seguem com o código próprio deles por decisão do dono do produto.
/// </summary>
public interface IInvitedUserService
{
    Task<User> InviteAsync(InviteUserCommand command, CancellationToken cancellationToken);
}

/// <summary>
/// Dados do convite. Os Vínculos vêm prontos porque quem decide o Escopo e o Perfil é a RN de
/// cada fluxo (RN-068: Tomador nomeado; RN-069: Corretora ativa) — este serviço não decide isso.
/// </summary>
/// <param name="Name">Nome do convidado.</param>
/// <param name="Email">E-mail do convidado (chave de unicidade na plataforma e no provedor).</param>
/// <param name="BrokerageMemberships">Vínculos de Corretora a criar: Corretora × Perfil.</param>
/// <param name="PolicyHolderMemberships">Vínculos de Tomador a criar: Tomador × Perfil.</param>
public sealed record InviteUserCommand(
    string Name,
    string Email,
    IReadOnlyCollection<ScopeMembership> BrokerageMemberships,
    IReadOnlyCollection<ScopeMembership> PolicyHolderMemberships);

/// <summary>Escopo (Corretora ou Tomador) e o Perfil que o convidado terá nele (RN-062).</summary>
public sealed record ScopeMembership(Guid ScopeId, Guid ProfileId);

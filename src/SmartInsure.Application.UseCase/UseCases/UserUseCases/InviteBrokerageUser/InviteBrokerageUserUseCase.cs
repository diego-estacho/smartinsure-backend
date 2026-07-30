using SmartInsure.Application.UseCase.Services.Invitations;
using SmartInsure.Application.UseCase.Services.Scopes;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.InviteBrokerageUser.Interfaces;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.InviteBrokerageUser.Requests;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.InviteBrokerageUser.Responses;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Constants;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.InviteBrokerageUser;

/// <summary>
/// RN-069 — o Corretor Administrador cria Usuários na Corretora ativa com um Perfil do Escopo
/// daquela Corretora: o Perfil fixo Corretor ou um Perfil customizado da própria Corretora
/// (RN-072). O Usuário nasce Pendente com Convite (RN-065), vinculado à Corretora ativa.
/// Criar outro Corretor Administrador não é deste fluxo — quem convida CA é o Administrador do
/// Sistema (RN-066).
/// </summary>
public sealed class InviteBrokerageUserUseCase(
    IScopeAuthorization scopeAuthorization,
    IProfileRepository profileRepository,
    IInvitedUserService invitedUserService) : IInviteBrokerageUserUseCase
{
    public async Task<InviteBrokerageUserResponse> ExecuteAsync(
        InviteBrokerageUserRequest request,
        CancellationToken cancellationToken)
    {
        var actor = await scopeAuthorization.RequireBrokerageAdministratorAsync(
            request.ExternalIdentity, request.ActiveBrokerageId, cancellationToken);

        var profile = await profileRepository.GetByIdAsync(request.ProfileId, cancellationToken)
            ?? throw new NotFoundException("Perfil não encontrado.");

        // RN-072: só Perfis do Escopo da Corretora ativa são atribuíveis aqui — Perfil de Sistema,
        // de Tomador ou customizado de outra Corretora não entra.
        if (profile.Scope != EProfileScope.Brokerage)
        {
            throw new BusinessRuleException("O perfil escolhido não é de escopo de Corretora.");
        }

        if (profile.BrokerageId is { } profileBrokerageId && profileBrokerageId != actor.ScopeId)
        {
            throw new BusinessRuleException("O perfil escolhido pertence a outra corretora.");
        }

        // RN-066: o Perfil Corretor Administrador é concedido pelo Administrador do Sistema.
        if (profile.IsFixed && profile.Name == ProfileNames.BrokerageAdministrator)
        {
            throw new BusinessRuleException(
                "O perfil Corretor Administrador é concedido pelo Administrador do Sistema.");
        }

        var user = await invitedUserService.InviteAsync(
            new InviteUserCommand(
                request.Name,
                request.Email,
                BrokerageMemberships: [new ScopeMembership(actor.ScopeId, profile.Id)],
                PolicyHolderMemberships: []),
            cancellationToken);

        return new InviteBrokerageUserResponse(
            user.Id,
            user.Name,
            user.Email,
            user.Status.ToString(),
            actor.ScopeId,
            profile.Id,
            profile.Name);
    }
}

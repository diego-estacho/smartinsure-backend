using SmartInsure.Application.UseCase.Services.Invitations;
using SmartInsure.Application.UseCase.Services.Scopes;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.InvitePolicyHolderUser.Interfaces;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.InvitePolicyHolderUser.Requests;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.InvitePolicyHolderUser.Responses;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Constants;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.InvitePolicyHolderUser;

/// <summary>
/// RN-070 — o Tomador Administrador cria Usuários do Tomador ativo, com o Perfil fixo Tomador ou
/// um Perfil customizado daquele Tomador (RN-072). O Usuário nasce Pendente com Convite (RN-065),
/// vinculado ao Tomador ativo. Criar outro Tomador Administrador não é deste fluxo — quem faz isso
/// é o Corretor Administrador (RN-068).
/// </summary>
public sealed class InvitePolicyHolderUserUseCase(
    IScopeAuthorization scopeAuthorization,
    IProfileRepository profileRepository,
    IInvitedUserService invitedUserService) : IInvitePolicyHolderUserUseCase
{
    public async Task<InvitePolicyHolderUserResponse> ExecuteAsync(
        InvitePolicyHolderUserRequest request,
        CancellationToken cancellationToken)
    {
        var actor = await scopeAuthorization.RequirePolicyHolderAdministratorAsync(
            request.ExternalIdentity, request.ActivePolicyHolderId, cancellationToken);

        var profile = await profileRepository.GetByIdAsync(request.ProfileId, cancellationToken)
            ?? throw new NotFoundException("Perfil não encontrado.");

        // RN-072: só Perfis do Escopo do Tomador ativo — de Sistema, de Corretora ou de outro
        // Tomador ficam fora.
        if (profile.Scope != EProfileScope.PolicyHolder)
        {
            throw new BusinessRuleException("O perfil escolhido não é de escopo de Tomador.");
        }

        // Perfil de outro dono (outro Tomador) é recurso alheio → 403 (Forbidden).
        // Diferente de escopo errado (422, BusinessRuleException) ou admin fora da hierarquia.
        if (profile.PolicyHolderId is { } profilePolicyHolderId
            && profilePolicyHolderId != actor.ScopeId)
        {
            throw new ForbiddenException("O perfil escolhido pertence a outro tomador.");
        }

        // RN-068: o Perfil Tomador Administrador é concedido pelo Corretor Administrador.
        if (profile.IsFixed && profile.Name == ProfileNames.PolicyHolderAdministrator)
        {
            throw new BusinessRuleException(
                "O perfil Tomador Administrador é concedido pelo Corretor Administrador.");
        }

        var user = await invitedUserService.InviteAsync(
            new InviteUserCommand(
                request.Name,
                request.Email,
                request.DocumentNumber,
                BrokerageMemberships: [],
                PolicyHolderMemberships: [new ScopeMembership(actor.ScopeId, profile.Id)]),
            cancellationToken);

        return new InvitePolicyHolderUserResponse(
            user.Id,
            user.Name,
            user.Email,
            user.Status.ToString(),
            actor.ScopeId,
            profile.Id,
            profile.Name);
    }
}

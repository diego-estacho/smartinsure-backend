using SmartInsure.Application.UseCase.Services.Invitations;
using SmartInsure.Application.UseCase.Services.Scopes;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.InvitePolicyHolderAdministrator.Interfaces;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.InvitePolicyHolderAdministrator.Requests;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.InvitePolicyHolderAdministrator.Responses;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Constants;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.InvitePolicyHolderAdministrator;

/// <summary>
/// RN-068 — o Corretor Administrador cria um Tomador Administrador. A atribuição exige que o
/// Tomador escolhido tenha Nomeação de Tomador Vigente em que a Corretora ativa é a nomeada,
/// em qualquer Seguradora. O Usuário nasce Pendente com Convite (RN-065) e Perfil Tomador
/// Administrador vinculado àquele Tomador.
/// </summary>
public sealed class InvitePolicyHolderAdministratorUseCase(
    IScopeAuthorization scopeAuthorization,
    IPersonRepository personRepository,
    IPolicyHolderAppointmentRepository appointmentRepository,
    IProfileRepository profileRepository,
    IInvitedUserService invitedUserService) : IInvitePolicyHolderAdministratorUseCase
{
    public async Task<InvitePolicyHolderAdministratorResponse> ExecuteAsync(
        InvitePolicyHolderAdministratorRequest request,
        CancellationToken cancellationToken)
    {
        // RN-068: quem cria é o Corretor Administrador, e a verificação usa sempre a Corretora ativa.
        var actor = await scopeAuthorization.RequireBrokerageAdministratorAsync(
            request.ExternalIdentity, request.ActiveBrokerageId, cancellationToken);

        var policyHolder = await personRepository.GetPolicyHolderByIdAsync(
            request.PolicyHolderId, cancellationToken)
            ?? throw new NotFoundException("Tomador não encontrado.");

        // RN-068: sem Nomeação Vigente com a Corretora ativa como nomeada, a criação é recusada.
        var hasActiveAppointment = await appointmentRepository
            .ExistsActiveForPolicyHolderAndBrokerageAsync(
                policyHolder.Id, actor.ScopeId, cancellationToken);

        if (!hasActiveAppointment)
        {
            throw new BusinessRuleException(
                "O tomador não tem nomeação vigente com a corretora ativa como nomeada.");
        }

        var policyHolderAdministrator = await profileRepository.GetByNameAsync(
            ProfileNames.PolicyHolderAdministrator, cancellationToken)
            ?? throw new BusinessRuleException(
                "Perfil Tomador Administrador não disponível na plataforma.");

        if (policyHolderAdministrator.Scope != EProfileScope.PolicyHolder)
        {
            throw new BusinessRuleException(
                "O perfil Tomador Administrador precisa ter escopo de Tomador.");
        }

        var user = await invitedUserService.InviteAsync(
            new InviteUserCommand(
                request.Name,
                request.Email,
                request.DocumentNumber,
                BrokerageMemberships: [],
                PolicyHolderMemberships:
                [
                    new ScopeMembership(policyHolder.Id, policyHolderAdministrator.Id),
                ]),
            cancellationToken);

        return new InvitePolicyHolderAdministratorResponse(
            user.Id, user.Name, user.Email, user.Status.ToString(), policyHolder.Id);
    }
}

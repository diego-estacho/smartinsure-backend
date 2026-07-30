using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Exceptions;
using SmartInsure.Infra.CrossCutting.Options;

namespace SmartInsure.Application.UseCase.Services.Invitations;

/// <summary>
/// RN-065 — Usuário convidado: nasce Pendente com Convite de uso único e recebe o link por e-mail.
/// A identidade é criada primeiro no provedor e desfeita se a gravação na plataforma falhar
/// (RN-001), para nunca existir Usuário sem identidade nem identidade órfã. O envio do e-mail é
/// pós-commit: falha de e-mail não desfaz a criação — o Convite fica reenviável.
/// </summary>
public sealed class InvitedUserService(
    IUserRepository userRepository,
    IInvitationRepository invitationRepository,
    IUserBrokerageMembershipRepository brokerageMembershipRepository,
    IUserPolicyHolderMembershipRepository policyHolderMembershipRepository,
    IIdentityProvider identityProvider,
    IInvitationMailer invitationMailer,
    IUnitOfWork unitOfWork,
    IOptions<InvitationOptions> invitationOptions,
    ILogger<InvitedUserService> logger) : IInvitedUserService
{
    private const string InvitationSubject = "Bem-vindo ao SmartInsure — Complete seu acesso";

    public async Task<User> InviteAsync(InviteUserCommand command, CancellationToken cancellationToken)
    {
        var email = command.Email.Trim().ToLowerInvariant();

        if (await userRepository.EmailExistsAsync(email, cancellationToken))
        {
            throw new ConflictException("Já existe um usuário com este e-mail na plataforma.");
        }

        if (await identityProvider.EmailExistsAsync(email, cancellationToken))
        {
            throw new ConflictException(
                "Já existe uma identidade com este e-mail no provedor de identidade.");
        }

        var externalIdentity = await identityProvider.CreateIdentityAsync(
            command.Name.Trim(), email, cancellationToken);

        User user;
        string plainToken;

        try
        {
            user = User.Create(command.Name, email, externalIdentity);
            await userRepository.AddAsync(user, cancellationToken);

            var (invitation, token) = Invitation.Create(user.Id, invitationOptions.Value.LinkExpiryDays);
            plainToken = token;
            await invitationRepository.AddAsync(invitation, cancellationToken);

            foreach (var membership in command.BrokerageMemberships)
            {
                await brokerageMembershipRepository.AddAsync(
                    UserBrokerageMembership.Create(user.Id, membership.ScopeId, membership.ProfileId),
                    cancellationToken);
            }

            foreach (var membership in command.PolicyHolderMemberships)
            {
                await policyHolderMembershipRepository.AddAsync(
                    UserPolicyHolderMembership.Create(user.Id, membership.ScopeId, membership.ProfileId),
                    cancellationToken);
            }

            await unitOfWork.CommitAsync(cancellationToken);
        }
        catch
        {
            // RN-001: gravação falhou → desfaz a identidade recém-criada, sem deixar órfã.
            try
            {
                await identityProvider.RemoveIdentityAsync(externalIdentity, CancellationToken.None);
            }
            catch (Exception compensationException)
            {
                logger.LogError(
                    compensationException,
                    "Falha ao remover identidade órfã no provedor de identidade. ExternalIdentity: {ExternalIdentity}",
                    externalIdentity);
            }

            throw;
        }

        try
        {
            await invitationMailer.SendAsync(
                email, user.Name, plainToken, InvitationSubject, cancellationToken);
        }
        catch (Exception emailException)
        {
            logger.LogError(
                emailException,
                "Falha ao enviar o convite para {Email}; o Usuário permanece Pendente (reenviável).",
                email);
        }

        return user;
    }
}

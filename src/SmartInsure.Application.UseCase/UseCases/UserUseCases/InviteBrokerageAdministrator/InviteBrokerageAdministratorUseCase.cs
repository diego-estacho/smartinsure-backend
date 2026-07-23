using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.InviteBrokerageAdministrator.Interfaces;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.InviteBrokerageAdministrator.Requests;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.InviteBrokerageAdministrator.Responses;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Abstractions.Services.Dtos;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;
using SmartInsure.Infra.CrossCutting.Options;

namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.InviteBrokerageAdministrator;

/// <summary>
/// RN-036 — o Administrador do Sistema convida um Corretor Administrador, informando as Corretoras.
/// O Usuário nasce Pendente com Convite de primeiro acesso (RN-035) e o Perfil fixo Corretor
/// Administrador em cada Corretora informada. A autorização (só Administrador do Sistema) é do endpoint.
/// </summary>
public sealed class InviteBrokerageAdministratorUseCase(
    IUserRepository userRepository,
    IPersonRepository personRepository,
    IProfileRepository profileRepository,
    IUserBrokerageMembershipRepository membershipRepository,
    IInvitationRepository invitationRepository,
    IIdentityProvider identityProvider,
    IMailService mailService,
    IUnitOfWork unitOfWork,
    IOptions<InvitationOptions> invitationOptions,
    ILogger<InviteBrokerageAdministratorUseCase> logger) : IInviteBrokerageAdministratorUseCase
{
    public async Task<InviteBrokerageAdministratorResponse> ExecuteAsync(
        InviteBrokerageAdministratorRequest request,
        CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (await userRepository.EmailExistsAsync(email, cancellationToken))
        {
            throw new ConflictException("Já existe um usuário com este e-mail na plataforma.");
        }

        if (await identityProvider.EmailExistsAsync(email, cancellationToken))
        {
            throw new ConflictException(
                "Já existe uma identidade com este e-mail no provedor de identidade.");
        }

        // RN-036: cada Corretora informada precisa existir (papel Broker) e estar Ativa.
        foreach (var brokerageId in request.BrokerageIds)
        {
            var brokerage = await personRepository.GetTrackedBrokerageByIdAsync(brokerageId, cancellationToken)
                ?? throw new NotFoundException("Corretora não encontrada.");

            if (brokerage.GetRole(EPersonRole.Broker)?.Status != EPersonRoleStatus.Active)
            {
                throw new BusinessRuleException("A corretora informada não está ativa.");
            }
        }

        var brokerageAdministrator = await profileRepository.GetBrokerageAdministratorAsync(cancellationToken)
            ?? throw new BusinessRuleException(
                "Perfil Corretor Administrador não disponível na plataforma.");

        var externalIdentity = await identityProvider.CreateIdentityAsync(
            request.Name.Trim(), email, cancellationToken);

        User user;
        string plainToken;

        try
        {
            user = User.Create(request.Name, email, externalIdentity);
            await userRepository.AddAsync(user, cancellationToken);

            // RN-035: Convite de primeiro acesso.
            var (invitation, token) = Invitation.Create(user.Id, invitationOptions.Value.LinkExpiryDays);
            plainToken = token;
            await invitationRepository.AddAsync(invitation, cancellationToken);

            // RN-036: um vínculo Corretor Administrador por Corretora informada.
            foreach (var brokerageId in request.BrokerageIds)
            {
                await membershipRepository.AddAsync(
                    UserBrokerageMembership.Create(user.Id, brokerageId, brokerageAdministrator.Id),
                    cancellationToken);
            }

            await unitOfWork.CommitAsync(cancellationToken);
        }
        catch
        {
            // RN-001: gravação na plataforma falhou → desfaz a identidade recém-criada, sem deixar órfã.
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

        // RN-035: link enviado pós-commit; falha de e-mail não desfaz a criação (reenviável).
        try
        {
            var invitationLink = $"{invitationOptions.Value.AppBaseUrl}/invite?token={Uri.EscapeDataString(plainToken)}";

            await mailService.SendAsync(
                new MailMessage
                {
                    To = [email],
                    Subject = "Bem-vindo ao SmartInsure — Complete seu acesso",
                    HtmlBody = BuildInvitationEmailHtml(user.Name, invitationLink),
                },
                cancellationToken);
        }
        catch (Exception emailException)
        {
            logger.LogError(
                emailException,
                "Falha ao enviar o convite para {Email}; o Usuário permanece Pendente (reenviável).",
                email);
        }

        return new InviteBrokerageAdministratorResponse(user.Id, user.Name, user.Email, user.Status.ToString());
    }

    private static string BuildInvitationEmailHtml(string userName, string invitationLink)
        => $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
        <h1>Bem-vindo ao SmartInsure, {System.Web.HttpUtility.HtmlEncode(userName)}!</h1>
        <p>Clique no link abaixo para completar seu acesso e definir sua senha:</p>
        <p style='text-align: center; margin: 30px 0;'>
            <a href='{System.Web.HttpUtility.HtmlAttributeEncode(invitationLink)}'
               style='display: inline-block; background-color: #0066cc; color: white; padding: 12px 30px; text-decoration: none; border-radius: 4px;'>
                Completar acesso
            </a>
        </p>
        <p style='color: #666; font-size: 12px;'>
            Este link expira em 7 dias. Se você não esperava este convite, ignore este e-mail.
        </p>
    </div>
</body>
</html>";
}

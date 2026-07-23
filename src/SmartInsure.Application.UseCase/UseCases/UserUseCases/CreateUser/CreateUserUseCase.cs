using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.CreateUser.Interfaces;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.CreateUser.Requests;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.CreateUser.Responses;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Abstractions.Services.Dtos;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Exceptions;
using SmartInsure.Infra.CrossCutting.Options;

namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.CreateUser;

/// <summary>
/// RN-001/RN-035 — Criação de Usuário: identidade criada primeiro no provedor de identidade;
/// falha ao gravar na plataforma desfaz a identidade (compensação). Nunca existe
/// Usuário sem identidade correspondente no provedor. RN-035 adiciona Convite por e-mail
/// no primeiro acesso (token de uso único).
/// </summary>
public sealed class CreateUserUseCase(
    IUserRepository userRepository,
    IInvitationRepository invitationRepository,
    IIdentityProvider identityProvider,
    IMailService mailService,
    IUnitOfWork unitOfWork,
    IOptions<InvitationOptions> invitationOptions,
    ILogger<CreateUserUseCase> logger) : ICreateUserUseCase
{
    /// <summary>
    /// Executa o caso de uso de criação de usuário com compensação de identidade em caso de falha.
    /// </summary>
    public async Task<CreateUserResponse> ExecuteAsync(
        CreateUserRequest request,
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

        var externalIdentity = await identityProvider.CreateIdentityAsync(
            request.Name.Trim(), email, cancellationToken);

        User user;
        string plainToken;

        try
        {
            user = User.Create(request.Name, email, externalIdentity);
            await userRepository.AddAsync(user, cancellationToken);

            // RN-035: Usuário e Convite gravados na mesma transação (atômico).
            var (invitation, token) = Invitation.Create(user.Id, invitationOptions.Value.LinkExpiryDays);
            plainToken = token;
            await invitationRepository.AddAsync(invitation, cancellationToken);
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

        // RN-035: envio do link é pós-commit. Falha de e-mail NÃO desfaz a criação — o Usuário
        // permanece Pendente e o Convite é reenviável; a falha é registrada.
        try
        {
            var invitationLink = $"{invitationOptions.Value.AppBaseUrl}/invite?token={Uri.EscapeDataString(plainToken)}";
            var htmlBody = BuildInvitationEmailHtml(user.Name, invitationLink);

            await mailService.SendAsync(
                new MailMessage
                {
                    To = [email],
                    Subject = "Bem-vindo ao SmartInsure — Complete seu acesso",
                    HtmlBody = htmlBody,
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

        return new CreateUserResponse(user.Id, user.Name, user.Email, user.Status.ToString());
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
            Este link expira em 7 dias. Se você não solicitou este convite, ignore este e-mail.
        </p>
    </div>
</body>
</html>";
}

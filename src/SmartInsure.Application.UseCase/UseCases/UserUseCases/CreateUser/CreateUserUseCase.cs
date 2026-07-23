using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartInsure.Application.UseCase.Services.Invitations;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.CreateUser.Interfaces;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.CreateUser.Requests;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.CreateUser.Responses;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Services;
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
    IInvitationMailer invitationMailer,
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
            await invitationMailer.SendAsync(
                email, user.Name, plainToken, "Bem-vindo ao SmartInsure — Complete seu acesso", cancellationToken);
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
}

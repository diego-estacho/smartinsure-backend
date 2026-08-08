using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartInsure.Application.UseCase.Services.Invitations;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.EditUser.Interfaces;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.EditUser.Requests;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.EditUser.Responses;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;
using SmartInsure.Infra.CrossCutting.Options;

namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.EditUser;

/// <summary>
/// RN-202 — Edição de Usuário: corrige o nome (sempre) e, SÓ enquanto Pendente (§9), o e-mail.
/// Trocar o e-mail atualiza a identidade no provedor (RN-005 valida por e-mail) e reenvia o Convite
/// para o novo endereço (RN-065), invalidando o link anterior; o Usuário segue Pendente. O histórico
/// é preservado. CPF (RN-082) é imutável e não é tocado aqui.
/// </summary>
public sealed class EditUserUseCase(
    IUserRepository userRepository,
    IInvitationRepository invitationRepository,
    IIdentityProvider identityProvider,
    IInvitationMailer invitationMailer,
    IUnitOfWork unitOfWork,
    IOptions<InvitationOptions> invitationOptions,
    ILogger<EditUserUseCase> logger) : IEditUserUseCase
{
    private const string InvitationSubject = "Bem-vindo ao SmartInsure — Complete seu acesso";

    public async Task<EditUserResponse> ExecuteAsync(
        EditUserRequest request,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException("Usuário não encontrado na plataforma.");

        user.Rename(request.Name);

        var newEmail = request.Email?.Trim().ToLowerInvariant();
        var emailChanged = !string.IsNullOrWhiteSpace(newEmail) && newEmail != user.Email;

        if (!emailChanged)
        {
            userRepository.Update(user);
            await unitOfWork.CommitAsync(cancellationToken);
            return new EditUserResponse(user.Id, user.Name, user.Email, user.Status.ToString());
        }

        // §9/RN-202: e-mail só muda enquanto Pendente — a entidade também trava, aqui a mensagem é clara.
        if (user.Status != EUserStatus.Pending)
        {
            throw new ConflictException(
                "O e-mail só pode ser alterado enquanto o usuário está pendente. Para trocar o e-mail "
                + "de um usuário ativo, inative-o e convide o novo endereço.");
        }

        if (await userRepository.EmailExistsAsync(newEmail!, cancellationToken))
        {
            throw new ConflictException("Já existe um usuário com este e-mail na plataforma.");
        }

        if (await identityProvider.EmailExistsAsync(newEmail!, cancellationToken))
        {
            throw new ConflictException(
                "Já existe uma identidade com este e-mail no provedor de identidade.");
        }

        // Provedor primeiro (RN-005 valida login por e-mail); depois a plataforma. Falha no commit
        // após atualizar o provedor deixa uma divergência recuperável (Usuário Pendente, sem sessão).
        await identityProvider.UpdateEmailAsync(user.ExternalIdentity, newEmail!, cancellationToken);
        user.ChangeEmail(newEmail!);

        // RN-065: reenvia o Convite para o novo endereço, invalidando o anterior.
        var oldInvitation = await invitationRepository.GetPendingByUserAsync(user.Id, cancellationToken);
        if (oldInvitation is not null)
        {
            oldInvitation.Consume();
            invitationRepository.Update(oldInvitation);
        }

        var (invitation, plainToken) = Invitation.Create(user.Id, invitationOptions.Value.LinkExpiryDays);
        await invitationRepository.AddAsync(invitation, cancellationToken);

        userRepository.Update(user);
        await unitOfWork.CommitAsync(cancellationToken);

        try
        {
            await invitationMailer.SendAsync(
                user.Email, user.Name, plainToken, InvitationSubject, cancellationToken);
        }
        catch (Exception emailException)
        {
            logger.LogError(
                emailException,
                "Falha ao reenviar o convite para o novo e-mail {Email}; o Usuário permanece Pendente (reenviável).",
                user.Email);
        }

        return new EditUserResponse(user.Id, user.Name, user.Email, user.Status.ToString());
    }
}

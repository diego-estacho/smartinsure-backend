using Microsoft.Extensions.Options;
using SmartInsure.Application.UseCase.Services.Invitations;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.ResendInvitation.Interfaces;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.ResendInvitation.Requests;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.ResendInvitation.Responses;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Exceptions;
using SmartInsure.Infra.CrossCutting.Options;

namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.ResendInvitation;

/// <summary>RN-035: reenvio do convite — invalida o anterior e envia novo por e-mail.</summary>
public sealed class ResendInvitationUseCase(
    IInvitationRepository invitationRepository,
    IUserRepository userRepository,
    IInvitationMailer invitationMailer,
    IUnitOfWork unitOfWork,
    IOptions<InvitationOptions> invitationOptions) : IResendInvitationUseCase
{
    public async Task<ResendInvitationResponse> ExecuteAsync(
        ResendInvitationRequest request,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken);

        if (user is null)
        {
            throw new NotFoundException("Usuário não encontrado.");
        }

        // RN-035: só Pendentes podem receber novo convite (a constraint DB garante um ativo).
        var oldInvitation = await invitationRepository.GetPendingByUserAsync(user.Id, cancellationToken);

        if (oldInvitation is not null)
        {
            // Invalida o anterior e PERSISTE antes de inserir o novo: o índice único filtrado
            // (um Convite ativo por Usuário) proíbe dois ativos, e o EF não garante UPDATE antes de INSERT
            // no mesmo commit — por isso o consume é gravado primeiro.
            oldInvitation.Consume();
            invitationRepository.Update(oldInvitation);
            await unitOfWork.CommitAsync(cancellationToken);
        }

        // RN-035: gera novo convite.
        var (newInvitation, plainToken) = Invitation.Create(user.Id, invitationOptions.Value.LinkExpiryDays);
        await invitationRepository.AddAsync(newInvitation, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        await invitationMailer.SendAsync(
            user.Email, user.Name, plainToken, "SmartInsure — Novo link de acesso", cancellationToken);

        return new ResendInvitationResponse(user.Id, user.Email);
    }
}

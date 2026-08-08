using Microsoft.Extensions.Options;
using SmartInsure.Application.UseCase.Services.Invitations;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.RequestPasswordReset.Interfaces;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.RequestPasswordReset.Requests;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.RequestPasswordReset.Responses;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;
using SmartInsure.Infra.CrossCutting.Options;

namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.RequestPasswordReset;

/// <summary>
/// RN-203 — redefinição de senha do Usuário Ativo (disparada pelo administrador). Gera um link de
/// uso único e validade (mesma mecânica do Convite, RN-065) e o envia por e-mail ao endereço do
/// Usuário; ao abrir o link e definir a nova senha, ela é atualizada no provedor (RN-005) e o
/// Usuário permanece Ativo. Um pedido de redefinição anterior ainda válido deixa de valer.
/// Só Usuário Ativo: Pendente usa o Convite (RN-065); Inativo precisa ser reativado antes (RN-076).
/// </summary>
public sealed class RequestPasswordResetUseCase(
    IInvitationRepository invitationRepository,
    IUserRepository userRepository,
    IInvitationMailer invitationMailer,
    IUnitOfWork unitOfWork,
    IOptions<InvitationOptions> invitationOptions) : IRequestPasswordResetUseCase
{
    public async Task<RequestPasswordResetResponse> ExecuteAsync(
        RequestPasswordResetRequest request,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException("Usuário não encontrado.");

        // RN-203: redefinição é para quem já tem senha (Ativo). Pendente ainda não fez o primeiro
        // acesso (usa o Convite, RN-065); Inativo não acessa a plataforma (reative antes, RN-076).
        if (user.Status != EUserStatus.Active)
        {
            throw new ConflictException(
                user.Status == EUserStatus.Pending
                    ? "Usuário pendente ainda não definiu senha — reenvie o convite de primeiro acesso."
                    : "Usuário inativo não recebe redefinição de senha — reative o acesso antes.");
        }

        // Invalida um pedido de redefinição anterior antes de inserir o novo: o índice único filtrado
        // (um token ativo por Usuário) proíbe dois ativos e o EF não garante UPDATE antes de INSERT no
        // mesmo commit — por isso o consume é gravado primeiro (mesmo padrão do reenvio, RN-065).
        var pending = await invitationRepository.GetPendingByUserAsync(user.Id, cancellationToken);
        if (pending is not null)
        {
            pending.Consume();
            invitationRepository.Update(pending);
            await unitOfWork.CommitAsync(cancellationToken);
        }

        var (invitation, plainToken) = Invitation.Create(user.Id, invitationOptions.Value.LinkExpiryDays);
        await invitationRepository.AddAsync(invitation, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        await invitationMailer.SendPasswordResetAsync(
            user.Email, user.Name, plainToken, cancellationToken);

        return new RequestPasswordResetResponse(user.Id, user.Email);
    }
}

using Microsoft.Extensions.Logging;
using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.AcceptInvitation.Interfaces;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.AcceptInvitation.Requests;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.AcceptInvitation.Responses;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.AcceptInvitation;

/// <summary>RN-035: aceite do convite de primeiro acesso — define a senha e ativa o Usuário.</summary>
public sealed class AcceptInvitationUseCase(
    IInvitationRepository invitationRepository,
    IUserRepository userRepository,
    IIdentityProvider identityProvider,
    IUnitOfWork unitOfWork,
    ILogger<AcceptInvitationUseCase> logger) : IAcceptInvitationUseCase
{
    public async Task<AcceptInvitationResponse> ExecuteAsync(
        AcceptInvitationRequest request,
        CancellationToken cancellationToken)
    {
        var tokenHash = HashToken(request.Token);
        var invitation = await invitationRepository.GetByTokenHashAsync(tokenHash, cancellationToken);

        if (invitation is null)
        {
            throw new NotFoundException("Convite inválido ou não encontrado.");
        }

        if (!invitation.IsValid())
        {
            throw new BusinessRuleException(
                "Convite expirado ou já foi aceito. Solicite um novo convite.");
        }

        var user = await userRepository.GetByIdAsync(invitation.UserId, cancellationToken);

        if (user is null)
        {
            logger.LogWarning("Convite órfão: Usuário {UserId} não encontrado.", invitation.UserId);
            throw new NotFoundException("Usuário não encontrado.");
        }

        // RN-035: define a senha no provedor de identidade.
        await identityProvider.SetPasswordAsync(
            user.ExternalIdentity, request.Password, cancellationToken);

        // RN-035: marca o convite como consumido.
        invitation.Consume();

        // RN-002: ativa o Usuário (agora que a senha está definida).
        user.Activate();

        invitationRepository.Update(invitation);
        userRepository.Update(user);
        await unitOfWork.CommitAsync(cancellationToken);

        return new AcceptInvitationResponse(user.Id, user.Name, user.Email, user.Status.ToString());
    }

    private static string HashToken(string plainToken)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hashData = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(plainToken));
        return System.Convert.ToHexString(hashData);
    }
}

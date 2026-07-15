using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.ActivateUser.Interfaces;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.ActivateUser.Requests;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.ActivateUser.Responses;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.ActivateUser;

/// <summary>
/// RN-002 — Ativação do Usuário no primeiro acesso: Pendente → Ativo somente após a
/// troca obrigatória da senha inicial padrão estar concluída no provedor de identidade.
/// </summary>
public sealed class ActivateUserUseCase(
    IUserRepository userRepository,
    IIdentityProvider identityProvider,
    IUnitOfWork unitOfWork) : IActivateUserUseCase
{
    /// <summary>Executa o caso de uso de ativação de usuário.</summary>
    public async Task<ActivateUserResponse> ExecuteAsync(
        ActivateUserRequest request,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByExternalIdentityAsync(
            request.ExternalIdentity, cancellationToken)
            ?? throw new NotFoundException("Usuário não encontrado na plataforma.");

        if (user.Status == EUserStatus.Active)
        {
            return new ActivateUserResponse(user.Id, user.Status.ToString());
        }

        if (await identityProvider.IsInitialPasswordPendingAsync(
            user.ExternalIdentity, cancellationToken))
        {
            throw new BusinessRuleException(
                "O usuário permanece pendente até concluir a troca da senha inicial padrão.");
        }

        user.Activate();
        userRepository.Update(user);
        await unitOfWork.CommitAsync(cancellationToken);

        return new ActivateUserResponse(user.Id, user.Status.ToString());
    }
}

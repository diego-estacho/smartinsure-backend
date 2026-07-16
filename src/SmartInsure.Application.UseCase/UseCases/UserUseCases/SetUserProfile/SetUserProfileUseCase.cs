using Microsoft.Extensions.Caching.Distributed;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.SetUserProfile.Interfaces;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.SetUserProfile.Requests;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.SetUserProfile.Responses;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Constants;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.SetUserProfile;

/// <summary>
/// RN-010 — concessão/revogação do Perfil; a plataforma nunca fica sem Administrador do
/// Sistema; cache de perfil invalidado para efeito imediato.
/// </summary>
public sealed class SetUserProfileUseCase(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IDistributedCache cache) : ISetUserProfileUseCase
{
    public async Task<SetUserProfileResponse> ExecuteAsync(
        SetUserProfileRequest request,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException("Usuário não encontrado na plataforma.");

        if (request.Profile is null)
        {
            if (user.Profile == EUserProfile.SystemAdministrator
                && await userRepository.CountByProfileAsync(
                    EUserProfile.SystemAdministrator, cancellationToken) <= 1)
            {
                throw new BusinessRuleException(
                    "A plataforma não pode ficar sem Administrador do Sistema.");
            }

            user.RevokeProfile();
        }
        else
        {
            if (!Enum.TryParse<EUserProfile>(request.Profile, ignoreCase: true, out var profile))
            {
                throw new BusinessRuleException("O perfil informado é desconhecido.");
            }

            user.GrantProfile(profile);
        }

        await unitOfWork.CommitAsync(cancellationToken);
        await cache.RemoveAsync(CacheKeys.UserProfile(user.ExternalIdentity), cancellationToken);

        return new SetUserProfileResponse(user.Id, user.Profile?.ToString());
    }
}

using Microsoft.Extensions.Caching.Distributed;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.SetUserProfile.Interfaces;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.SetUserProfile.Requests;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.SetUserProfile.Responses;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Constants;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.SetUserProfile;

/// <summary>
/// RN-012 — concessão/revogação do Perfil (agora entidade — RN-032); a plataforma nunca fica sem
/// Administrador do Sistema; cache de perfil invalidado para efeito imediato.
/// </summary>
public sealed class SetUserProfileUseCase(
    IUserRepository userRepository,
    IProfileRepository profileRepository,
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
            var systemAdministrator = await profileRepository.GetSystemAdministratorAsync(cancellationToken);

            if (systemAdministrator is not null
                && user.ProfileId == systemAdministrator.Id
                && await userRepository.CountByProfileIdAsync(systemAdministrator.Id, cancellationToken) <= 1)
            {
                throw new BusinessRuleException(
                    "A plataforma não pode ficar sem Administrador do Sistema.");
            }

            user.RevokeProfile();
        }
        else
        {
            var profile = await profileRepository.GetByNameAsync(request.Profile, cancellationToken)
                ?? throw new BusinessRuleException(
                    "O perfil deve ser SystemAdministrator ou nulo para revogação.");

            user.GrantProfile(profile);
        }

        await unitOfWork.CommitAsync(cancellationToken);
        await cache.RemoveAsync(CacheKeys.UserProfile(user.ExternalIdentity), cancellationToken);

        return new SetUserProfileResponse(user.Id, user.Profile?.Name);
    }
}

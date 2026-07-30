using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.Services.Scopes;
using SmartInsure.Application.UseCase.UseCases.ProfileUseCases.DeleteScopedProfile.Interfaces;
using SmartInsure.Application.UseCase.UseCases.ProfileUseCases.DeleteScopedProfile.Requests;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.UseCases.ProfileUseCases.DeleteScopedProfile;

/// <summary>
/// RN-074 — remoção de Perfil customizado pelo administrador do seu Escopo. A remoção é recusada
/// enquanto houver Usuário com aquele Perfil: primeiro os Usuários vão para outro Perfil (RN-075).
/// Perfil fixo da plataforma nunca é removido.
/// </summary>
public sealed class DeleteScopedProfileUseCase(
    IScopeAuthorization scopeAuthorization,
    IProfileRepository profileRepository,
    IUnitOfWork unitOfWork) : IDeleteScopedProfileUseCase
{
    public async Task<Unit> ExecuteAsync(
        DeleteScopedProfileRequest request,
        CancellationToken cancellationToken)
    {
        var scope = await scopeAuthorization.RequireScopeAdministratorAsync(
            request.ExternalIdentity,
            request.ActiveBrokerageId,
            request.ActivePolicyHolderId,
            cancellationToken);

        var profile = await profileRepository.GetTrackedByIdAsync(request.ProfileId, cancellationToken)
            ?? throw new NotFoundException("Perfil não encontrado.");

        // Perfil fixo é global (sem dono): a recusa por ser fixo vem primeiro, senão a checagem de
        // Escopo devolveria "não pertence ao seu escopo" e esconderia o motivo real.
        if (profile.IsFixed)
        {
            throw new BusinessRuleException("Perfil fixo da plataforma não pode ser removido.");
        }

        var owner = profile.Scope == EProfileScope.Brokerage
            ? profile.BrokerageId
            : profile.PolicyHolderId;

        if (profile.Scope != scope.Scope || owner != scope.OwnerId)
        {
            throw new ForbiddenException("Este perfil não pertence ao escopo que você administra.");
        }

        var usersWithProfile = await profileRepository.CountUsersByProfileAsync(
            profile.Id, cancellationToken);

        if (usersWithProfile > 0)
        {
            throw new ConflictException(
                "O perfil está em uso por usuários e não pode ser removido. Mova os usuários para outro perfil antes.");
        }

        profileRepository.Remove(profile);
        await unitOfWork.CommitAsync(cancellationToken);

        return Unit.Value;
    }
}

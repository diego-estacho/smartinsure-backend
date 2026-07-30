using SmartInsure.Application.UseCase.Services.Scopes;
using SmartInsure.Application.UseCase.UseCases.ProfileUseCases.GetProfile.Interfaces;
using SmartInsure.Application.UseCase.UseCases.ProfileUseCases.GetProfile.Requests;
using SmartInsure.Application.UseCase.UseCases.ProfileUseCases.GetProfile.Responses;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Repositories.Dtos;
using SmartInsure.Core.Constants;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.UseCases.ProfileUseCases.GetProfile;

/// <summary>
/// Detalhe do Perfil com as Permissões marcadas (RN-062/RN-063). Perfil sem Permissão é válido —
/// devolve lista vazia, não erro. A visibilidade segue a RN-072: administrador de Escopo só vê os
/// Perfis do próprio Escopo, e os fixos de administração ficam fora mesmo por consulta direta.
/// </summary>
public sealed class GetProfileUseCase(
    IUserRepository userRepository,
    IProfileRepository profileRepository,
    IScopeAuthorization scopeAuthorization) : IGetProfileUseCase
{
    public async Task<GetProfileResponse> ExecuteAsync(
        GetProfileRequest request,
        CancellationToken cancellationToken)
    {
        var profile = await profileRepository.GetDetailsByIdAsync(request.ProfileId, cancellationToken)
            ?? throw new NotFoundException("Perfil não encontrado.");

        await EnsureVisibleAsync(profile, request, cancellationToken);

        return new GetProfileResponse(
            profile.Id,
            profile.Name,
            profile.Scope,
            profile.IsFixed,
            profile.BrokerageId,
            profile.PolicyHolderId,
            profile.Permissions
                .Select(permission => new ProfilePermissionResponse(
                    permission.Id,
                    permission.Code,
                    permission.Description,
                    permission.IsSystem))
                .ToList());
    }

    private async Task EnsureVisibleAsync(
        ProfileDetailsDto profile,
        GetProfileRequest request,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByExternalIdentityAsync(
            request.ExternalIdentity, cancellationToken)
            ?? throw new NotFoundException("Usuário não encontrado.");

        if (user.Profile?.Name == ProfileNames.SystemAdministrator)
        {
            return;
        }

        // RN-072: para quem não é Administrador do Sistema, Perfil fixo de administração não existe
        // na gestão — nem listado, nem por identificador.
        if (profile.IsFixed && ProfileNames.AdministrativeFixed.Contains(profile.Name))
        {
            throw new NotFoundException("Perfil não encontrado.");
        }

        var administered = await scopeAuthorization.RequireScopeAdministratorAsync(
            request.ExternalIdentity,
            request.ActiveBrokerageId,
            request.ActivePolicyHolderId,
            cancellationToken);

        var scope = Enum.Parse<EProfileScope>(profile.Scope);
        var owner = scope == EProfileScope.Brokerage ? profile.BrokerageId : profile.PolicyHolderId;

        // Perfil global do próprio tipo de Escopo (dono nulo) é visível; de outro dono, não.
        if (scope != administered.Scope || (owner is not null && owner != administered.OwnerId))
        {
            throw new ForbiddenException("Este perfil não pertence ao escopo que você administra.");
        }
    }
}

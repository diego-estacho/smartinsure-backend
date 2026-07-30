using SmartInsure.Application.UseCase.Services.Scopes;
using SmartInsure.Application.UseCase.UseCases.ProfileUseCases.UpdateScopedProfile.Interfaces;
using SmartInsure.Application.UseCase.UseCases.ProfileUseCases.UpdateScopedProfile.Requests;
using SmartInsure.Application.UseCase.UseCases.ProfileUseCases.UpdateScopedProfile.Responses;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.UseCases.ProfileUseCases.UpdateScopedProfile;

/// <summary>
/// RN-074 — o Perfil customizado é editado dentro do seu Escopo: os de Corretora pelo Corretor
/// Administrador daquela Corretora; os de Tomador pelo Tomador Administrador daquele Tomador.
/// Perfil fixo não é editado aqui (nome/Escopo são imutáveis; Permissões são RN-073, do
/// Administrador do Sistema).
/// </summary>
public sealed class UpdateScopedProfileUseCase(
    IScopeAuthorization scopeAuthorization,
    IProfileRepository profileRepository,
    IPermissionRepository permissionRepository,
    IUnitOfWork unitOfWork) : IUpdateScopedProfileUseCase
{
    public async Task<UpdateScopedProfileResponse> ExecuteAsync(
        UpdateScopedProfileRequest request,
        CancellationToken cancellationToken)
    {
        var scope = await scopeAuthorization.RequireScopeAdministratorAsync(
            request.ExternalIdentity,
            request.ActiveBrokerageId,
            request.ActivePolicyHolderId,
            cancellationToken);

        var profile = await profileRepository.GetTrackedByIdAsync(request.ProfileId, cancellationToken)
            ?? throw new NotFoundException("Perfil não encontrado.");

        // RN-073 antes da checagem de Escopo: Perfil fixo é global (sem dono), então a recusa por
        // Escopo mascararia o motivo real — o que impede a edição é ele ser fixo da plataforma.
        if (profile.IsFixed)
        {
            throw new BusinessRuleException(
                "Perfil fixo da plataforma não é editado pelo administrador do escopo.");
        }

        EnsureBelongsToScope(profile, scope);

        var nameInUse = await profileRepository.ExistsByNameInScopeAsync(
            request.Name, scope.Scope, scope.OwnerId, profile.Id, cancellationToken);

        if (nameInUse)
        {
            throw new ConflictException("Já existe um perfil com este nome neste escopo.");
        }

        var permissions = await ResolvePermissionsAsync(request.PermissionCodes, cancellationToken);

        profile.Rename(request.Name);
        profile.ReplacePermissions(permissions);

        profileRepository.Update(profile);
        await unitOfWork.CommitAsync(cancellationToken);

        return new UpdateScopedProfileResponse(
            profile.Id, profile.Name, profile.Scope.ToString(), profile.Permissions.Count);
    }

    /// <summary>
    /// RN-074: administrador de um Escopo não toca Perfil de outro — nem de outra Corretora/Tomador,
    /// nem global (o global é da plataforma).
    /// </summary>
    private static void EnsureBelongsToScope(Profile profile, AdministeredScope scope)
    {
        var owner = profile.Scope == EProfileScope.Brokerage
            ? profile.BrokerageId
            : profile.PolicyHolderId;

        if (profile.Scope != scope.Scope || owner != scope.OwnerId)
        {
            throw new ForbiddenException("Este perfil não pertence ao escopo que você administra.");
        }
    }

    private async Task<IReadOnlyCollection<Permission>> ResolvePermissionsAsync(
        IReadOnlyCollection<string> codes,
        CancellationToken cancellationToken)
    {
        if (codes.Count == 0)
        {
            return [];
        }

        var permissions = await permissionRepository.GetByCodesAsync(codes, cancellationToken);
        var unknown = codes
            .Select(code => code.Trim())
            .Distinct()
            .Except(permissions.Select(permission => permission.Code))
            .ToList();

        if (unknown.Count > 0)
        {
            throw new BusinessRuleException(
                $"Permissão fora do catálogo da plataforma: {string.Join(", ", unknown)}.");
        }

        return permissions;
    }
}

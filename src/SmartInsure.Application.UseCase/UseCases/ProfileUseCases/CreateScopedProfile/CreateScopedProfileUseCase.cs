using SmartInsure.Application.UseCase.Services.Scopes;
using SmartInsure.Application.UseCase.UseCases.ProfileUseCases.CreateScopedProfile.Interfaces;
using SmartInsure.Application.UseCase.UseCases.ProfileUseCases.CreateScopedProfile.Requests;
using SmartInsure.Application.UseCase.UseCases.ProfileUseCases.CreateScopedProfile.Responses;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.UseCases.ProfileUseCases.CreateScopedProfile;

/// <summary>
/// RN-069/RN-070 — Perfil customizado criado no Escopo do administrador: o Corretor Administrador
/// cria Perfis da Corretora ativa; o Tomador Administrador, Perfis do Tomador ativo. O Perfil nasce
/// vinculado àquele Escopo e passa a ser oferecido na criação de Usuários dele (RN-072). Nome
/// repetido no mesmo Escopo é recusado; as Permissões vêm do catálogo fixo (RN-063).
/// </summary>
public sealed class CreateScopedProfileUseCase(
    IScopeAuthorization scopeAuthorization,
    IProfileRepository profileRepository,
    IPermissionRepository permissionRepository,
    IUnitOfWork unitOfWork) : ICreateScopedProfileUseCase
{
    public async Task<CreateScopedProfileResponse> ExecuteAsync(
        CreateScopedProfileRequest request,
        CancellationToken cancellationToken)
    {
        var scope = await scopeAuthorization.RequireScopeAdministratorAsync(
            request.ExternalIdentity,
            request.ActiveBrokerageId,
            request.ActivePolicyHolderId,
            cancellationToken);

        var nameInUse = await profileRepository.ExistsByNameInScopeAsync(
            request.Name, scope.Scope, scope.OwnerId, null, cancellationToken);

        if (nameInUse)
        {
            throw new ConflictException("Já existe um perfil com este nome neste escopo.");
        }

        var permissions = await ResolvePermissionsAsync(request.PermissionCodes, cancellationToken);

        var profile = scope.Scope == EProfileScope.Brokerage
            ? Profile.CreateForBrokerage(request.Name, scope.OwnerId)
            : Profile.CreateForPolicyHolder(request.Name, scope.OwnerId);

        foreach (var permission in permissions)
        {
            profile.AddPermission(permission);
        }

        await profileRepository.AddAsync(profile, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return new CreateScopedProfileResponse(
            profile.Id,
            profile.Name,
            profile.Scope.ToString(),
            profile.BrokerageId,
            profile.PolicyHolderId,
            profile.Permissions.Count);
    }

    /// <summary>RN-063: só Permissão declarada no catálogo entra num Perfil.</summary>
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

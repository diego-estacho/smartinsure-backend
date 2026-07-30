using Microsoft.Extensions.Logging;
using SmartInsure.Application.UseCase.UseCases.ProfileUseCases.UpdateFixedProfilePermissions.Interfaces;
using SmartInsure.Application.UseCase.UseCases.ProfileUseCases.UpdateFixedProfilePermissions.Requests;
using SmartInsure.Application.UseCase.UseCases.ProfileUseCases.UpdateFixedProfilePermissions.Responses;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.UseCases.ProfileUseCases.UpdateFixedProfilePermissions;

/// <summary>
/// RN-073 — o Administrador do Sistema marca e desmarca Permissões dos Perfis fixos. A mudança vale
/// **globalmente**: passa a valer para todos os Usuários que têm aquele Perfil, em qualquer Corretora
/// ou Tomador. Nome, Escopo e estrutura do Perfil fixo seguem imutáveis; Perfil customizado é
/// editado pelo administrador do seu Escopo (RN-074), não aqui.
///
/// Efeito de remover uma Permissão essencial à própria administração (ex.: gerenciar Usuários no
/// Corretor Administrador) segue **não definido** nesta fase — [OPEN-18]. A operação é registrada em
/// log para dar rastro até a decisão existir.
/// </summary>
public sealed class UpdateFixedProfilePermissionsUseCase(
    IProfileRepository profileRepository,
    IPermissionRepository permissionRepository,
    IUnitOfWork unitOfWork,
    ILogger<UpdateFixedProfilePermissionsUseCase> logger) : IUpdateFixedProfilePermissionsUseCase
{
    public async Task<UpdateFixedProfilePermissionsResponse> ExecuteAsync(
        UpdateFixedProfilePermissionsRequest request,
        CancellationToken cancellationToken)
    {
        var profile = await profileRepository.GetTrackedByIdAsync(request.ProfileId, cancellationToken)
            ?? throw new NotFoundException("Perfil não encontrado.");

        if (!profile.IsFixed)
        {
            throw new BusinessRuleException(
                "Este fluxo edita apenas Perfis fixos da plataforma; perfil customizado é editado no seu escopo.");
        }

        var permissions = await ResolvePermissionsAsync(request.PermissionCodes, cancellationToken);
        var previousCount = profile.Permissions.Count;

        profile.ReplacePermissions(permissions);

        profileRepository.Update(profile);
        await unitOfWork.CommitAsync(cancellationToken);

        // OPEN-18: sem decisão sobre remover Permissão essencial à administração — deixa rastro.
        logger.LogInformation(
            "Permissões do perfil fixo {ProfileName} alteradas de {PreviousCount} para {CurrentCount} (efeito global, RN-073).",
            profile.Name,
            previousCount,
            profile.Permissions.Count);

        return new UpdateFixedProfilePermissionsResponse(
            profile.Id, profile.Name, profile.Scope.ToString(), profile.Permissions.Count);
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

using SmartInsure.Application.UseCase.UseCases.PermissionUseCases.ListPermissions.Interfaces;
using SmartInsure.Application.UseCase.UseCases.PermissionUseCases.ListPermissions.Requests;
using SmartInsure.Application.UseCase.UseCases.PermissionUseCases.ListPermissions.Responses;
using SmartInsure.Core.Abstractions.Repositories;

namespace SmartInsure.Application.UseCase.UseCases.PermissionUseCases.ListPermissions;

/// <summary>
/// RN-063 — catálogo fixo de Permissões declarado pela plataforma. É a lista oferecida na edição
/// de qualquer Perfil; ninguém cria Permissão por tela, então não há escrita correspondente.
/// </summary>
public sealed class ListPermissionsUseCase(IPermissionRepository permissionRepository)
    : IListPermissionsUseCase
{
    public async Task<IReadOnlyList<PermissionResponse>> ExecuteAsync(
        ListPermissionsRequest request,
        CancellationToken cancellationToken)
    {
        var permissions = await permissionRepository.ListAllAsync(cancellationToken);

        return permissions
            .Select(permission => new PermissionResponse(
                permission.Id,
                permission.Code,
                permission.Description,
                permission.IsSystem,
                permission.Area,
                permission.DependsOn))
            .ToList();
    }
}

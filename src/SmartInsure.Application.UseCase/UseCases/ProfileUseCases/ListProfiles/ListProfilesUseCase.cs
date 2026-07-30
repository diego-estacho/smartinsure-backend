using SmartInsure.Application.UseCase.ModelsBase;
using SmartInsure.Application.UseCase.Services.Scopes;
using SmartInsure.Application.UseCase.UseCases.ProfileUseCases.ListProfiles.Interfaces;
using SmartInsure.Application.UseCase.UseCases.ProfileUseCases.ListProfiles.Requests;
using SmartInsure.Application.UseCase.UseCases.ProfileUseCases.ListProfiles.Responses;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Constants;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.UseCases.ProfileUseCases.ListProfiles;

/// <summary>
/// Gestão de Perfis (RN-062/RN-072): o Administrador do Sistema vê todos os Perfis; o Corretor
/// Administrador e o Tomador Administrador veem apenas os do próprio Escopo e **nunca** os Perfis
/// fixos de administração (Administrador do Sistema, Corretor Administrador, Tomador
/// Administrador) — esses continuam atribuíveis pela hierarquia (RN-068/069/070), só não são
/// administráveis por eles. Quem não administra Escopo algum não acessa a gestão.
/// </summary>
public sealed class ListProfilesUseCase(
    IUserRepository userRepository,
    IProfileRepository profileRepository,
    IScopeAuthorization scopeAuthorization) : IListProfilesUseCase
{
    public async Task<PagedResponse<ProfileListItemResponse>> ExecuteAsync(
        ListProfilesRequest request,
        CancellationToken cancellationToken)
    {
        var page = Math.Max(request.Page, 1);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var scopeFilter = ParseScope(request.Scope);

        var user = await userRepository.GetByExternalIdentityAsync(
            request.ExternalIdentity, cancellationToken)
            ?? throw new NotFoundException("Usuário não encontrado.");

        if (user.Profile?.Name == ProfileNames.SystemAdministrator)
        {
            var (items, totalCount) = await profileRepository.ListAsync(
                page, pageSize, request.Search, scopeFilter, cancellationToken);

            return new PagedResponse<ProfileListItemResponse>(
                items.Select(Map).ToList(), page, pageSize, totalCount);
        }

        var administered = await scopeAuthorization.RequireScopeAdministratorAsync(
            request.ExternalIdentity,
            request.ActiveBrokerageId,
            request.ActivePolicyHolderId,
            cancellationToken);

        var scopeProfiles = await profileRepository.ListByScopeAsync(
            administered.Scope, administered.OwnerId, cancellationToken);

        // RN-072: os fixos de administração não aparecem na gestão para CA/TA.
        var visible = scopeProfiles
            .Where(profile => !(profile.IsFixed
                && ProfileNames.AdministrativeFixed.Contains(profile.Name)))
            .Where(profile => MatchesSearch(profile, request.Search))
            .OrderBy(profile => profile.Name)
            .ToList();

        var paged = visible
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(profile => new ProfileListItemResponse(
                profile.Id,
                profile.Name,
                profile.Scope.ToString(),
                profile.IsFixed,
                profile.BrokerageId,
                profile.PolicyHolderId,
                profile.Permissions.Count))
            .ToList();

        return new PagedResponse<ProfileListItemResponse>(paged, page, pageSize, visible.Count);
    }

    private static bool MatchesSearch(Profile profile, string? search)
        => string.IsNullOrWhiteSpace(search)
            || profile.Name.Contains(search.Trim(), StringComparison.OrdinalIgnoreCase);

    private static ProfileListItemResponse Map(Core.Abstractions.Repositories.Dtos.ProfileListItemDto item)
        => new(
            item.Id,
            item.Name,
            item.Scope,
            item.IsFixed,
            item.BrokerageId,
            item.PolicyHolderId,
            item.PermissionCount);

    /// <summary>Filtro pelo nome estável do Escopo (ADR-031); valor fora do enum é recusado.</summary>
    private static EProfileScope? ParseScope(string? scope)
    {
        if (string.IsNullOrWhiteSpace(scope))
        {
            return null;
        }

        if (!Enum.TryParse<EProfileScope>(scope.Trim(), ignoreCase: true, out var parsed))
        {
            throw new BusinessRuleException($"Escopo de perfil inválido: {scope}.");
        }

        return parsed;
    }
}

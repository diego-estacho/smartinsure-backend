using SmartInsure.Application.UseCase.Services.Scopes;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.ListUsers.Interfaces;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.ListUsers.Requests;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.ListUsers.Responses;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Repositories.Dtos;
using SmartInsure.Core.Constants;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.ListUsers;

/// <summary>
/// Listagem de Usuários (RN-001/RN-012) restrita ao Escopo de quem consulta (RN-064):
/// o Administrador do Sistema vê todos; o Corretor Administrador vê os Usuários com Vínculo na
/// Corretora ativa; o Tomador Administrador, os do Tomador ativo. Quem não administra Escopo
/// algum não lista Usuários — a Permissão de consulta para Usuário comum é a RN-071 (adiada).
/// </summary>
public sealed class ListUsersUseCase(
    IUserRepository userRepository,
    IScopeAuthorization scopeAuthorization) : IListUsersUseCase
{
    public async Task<ListUsersResponse> ExecuteAsync(
        ListUsersRequest request,
        CancellationToken cancellationToken)
    {
        var page = Math.Max(request.Page, 1);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var (brokerageId, policyHolderId) = await ResolveVisibleScopeAsync(request, cancellationToken);

        var filters = new UserListFilters
        {
            Search = request.Search,
            Status = ParseStatusFilter(request.Status),
            VisibleBrokerageId = brokerageId,
            VisiblePolicyHolderId = policyHolderId,
            ProfileId = request.ProfileId,
            Scope = ParseScope(request.Scope),
            LinkId = request.LinkId,
            RegisteredFrom = request.RegisteredFrom,
            RegisteredTo = request.RegisteredTo,
        };

        var (items, totalCount, counts) = await userRepository.ListAsync(
            page, pageSize, filters, cancellationToken);

        var responses = items
            .Select(item => new UserListItemResponse(
                item.Id,
                item.Name,
                item.Email,
                item.Status,
                item.ProfileName,
                item.ProfileScope,
                item.ProfileIsFixed,
                item.Link,
                item.CreatedAt,
                item.InviteExpired,
                item.LastAccessAtUtc))
            .ToList();

        return new ListUsersResponse(
            responses,
            page,
            pageSize,
            totalCount,
            new UserStatusCountsResponse(
                counts.All,
                counts.Active,
                counts.PendingNotExpired,
                counts.Expired,
                counts.Inactive));
    }

    /// <summary>
    /// RN-064: o Escopo da consulta. Administrador do Sistema não tem Escopo (vê tudo); os
    /// administradores de Escopo veem só o próprio. A conferência do Perfil é do servidor.
    /// </summary>
    private async Task<(Guid? BrokerageId, Guid? PolicyHolderId)> ResolveVisibleScopeAsync(
        ListUsersRequest request,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByExternalIdentityAsync(
            request.ExternalIdentity, cancellationToken)
            ?? throw new NotFoundException("Usuário não encontrado.");

        if (user.Profile?.Name == ProfileNames.SystemAdministrator)
        {
            return (null, null);
        }

        if (request.ActiveBrokerageId is { } brokerageId)
        {
            await scopeAuthorization.RequireBrokerageAdministratorAsync(
                request.ExternalIdentity, brokerageId, cancellationToken);

            return (brokerageId, null);
        }

        if (request.ActivePolicyHolderId is { } policyHolderId)
        {
            await scopeAuthorization.RequirePolicyHolderAdministratorAsync(
                request.ExternalIdentity, policyHolderId, cancellationToken);

            return (null, policyHolderId);
        }

        throw new ForbiddenException(
            "Selecione a corretora ou o tomador ativo para consultar usuários.");
    }

    /// <summary>
    /// Filtro de situação da tela: `Active`/`Inactive` casam com o enum; `Pending` é o Pendente
    /// não expirado e `Expired` é o Pendente com Convite vencido (RN-065). Vazio = todas.
    /// </summary>
    private static EUserListStatusFilter? ParseStatusFilter(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return null;
        }

        return status.Trim().ToLowerInvariant() switch
        {
            "all" or "todos" => null,
            "active" or "ativo" => EUserListStatusFilter.Active,
            "inactive" or "inativo" => EUserListStatusFilter.Inactive,
            "pending" or "pendente" => EUserListStatusFilter.PendingNotExpired,
            "expired" or "expirado" => EUserListStatusFilter.Expired,
            _ => throw new BusinessRuleException($"Situação de usuário inválida: {status}."),
        };
    }

    /// <summary>Filtro de Escopo (§4): nome estável do enum (System/Brokerage/PolicyHolder). Vazio = todos.</summary>
    private static EProfileScope? ParseScope(string? scope)
    {
        if (string.IsNullOrWhiteSpace(scope))
        {
            return null;
        }

        if (!Enum.TryParse<EProfileScope>(scope.Trim(), ignoreCase: true, out var parsed))
        {
            throw new BusinessRuleException($"Escopo inválido: {scope}.");
        }

        return parsed;
    }
}

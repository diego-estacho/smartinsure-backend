using SmartInsure.Application.UseCase.ModelsBase;
using SmartInsure.Application.UseCase.Services.Scopes;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.ListUsers.Interfaces;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.ListUsers.Requests;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.ListUsers.Responses;
using SmartInsure.Core.Abstractions.Repositories;
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
    public async Task<PagedResponse<UserListItemResponse>> ExecuteAsync(
        ListUsersRequest request,
        CancellationToken cancellationToken)
    {
        var page = Math.Max(request.Page, 1);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var status = ParseStatus(request.Status);

        var (brokerageId, policyHolderId) = await ResolveVisibleScopeAsync(request, cancellationToken);

        var (items, totalCount) = await userRepository.ListAsync(
            page, pageSize, request.Search, status, brokerageId, policyHolderId, cancellationToken);

        var responses = items
            .Select(item => new UserListItemResponse(
                item.Id,
                item.Name,
                item.Email,
                item.Status,
                item.ProfileName,
                item.CreatedAt))
            .ToList();

        return new PagedResponse<UserListItemResponse>(responses, page, pageSize, totalCount);
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

        throw new UnauthorizedException(
            "Selecione a corretora ou o tomador ativo para consultar usuários.");
    }

    /// <summary>Filtro pelo nome estável da situação (ADR-031); valor fora do enum é recusado.</summary>
    private static EUserStatus? ParseStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return null;
        }

        if (!Enum.TryParse<EUserStatus>(status.Trim(), ignoreCase: true, out var parsed))
        {
            throw new BusinessRuleException($"Situação de usuário inválida: {status}.");
        }

        return parsed;
    }
}

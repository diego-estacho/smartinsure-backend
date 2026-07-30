using SmartInsure.Application.UseCase.UseCases.UserUseCases.GetCurrentUserContext.Interfaces;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.GetCurrentUserContext.Requests;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.GetCurrentUserContext.Responses;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Repositories.Dtos;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.GetCurrentUserContext;

/// <summary>
/// RN-064 — contexto do Usuário autenticado: Perfil de Escopo Sistema (RN-012), Vínculos de
/// Corretora e de Tomador com o Perfil de cada um, e qual deles está ativo no acesso corrente.
/// </summary>
public sealed class GetCurrentUserContextUseCase(IUserRepository userRepository)
    : IGetCurrentUserContextUseCase
{
    public async Task<GetCurrentUserContextResponse> ExecuteAsync(
        GetCurrentUserContextRequest request,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByExternalIdentityAsync(
            request.ExternalIdentity, cancellationToken)
            ?? throw new NotFoundException("Usuário não encontrado.");

        var details = await userRepository.GetDetailsByIdAsync(user.Id, cancellationToken)
            ?? throw new NotFoundException("Usuário não encontrado.");

        return new GetCurrentUserContextResponse(
            details.Id,
            details.Name,
            details.Email,
            details.Status,
            details.ProfileName,
            request.ActiveBrokerageId,
            request.ActivePolicyHolderId,
            Map(details.BrokerageMemberships, request.ActiveBrokerageId),
            Map(details.PolicyHolderMemberships, request.ActivePolicyHolderId));
    }

    private static List<UserScopeResponse> Map(
        IReadOnlyList<UserMembershipDto> memberships,
        Guid? activeScopeId)
        => memberships
            .Select(membership => new UserScopeResponse(
                membership.ScopeId,
                membership.ScopeDocumentNumber,
                membership.ScopeName,
                membership.ProfileName,
                membership.ScopeId == activeScopeId))
            .ToList();
}

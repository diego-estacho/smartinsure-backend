using SmartInsure.Application.UseCase.UseCases.UserUseCases.GetUser.Interfaces;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.GetUser.Requests;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.GetUser.Responses;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.GetUser;

/// <summary>
/// Detalhe do Usuário: Perfil de Escopo System (RN-012) e Vínculos de Corretora e Tomador
/// com o Perfil de cada Escopo (RN-064).
/// </summary>
public sealed class GetUserUseCase(IUserRepository userRepository) : IGetUserUseCase
{
    public async Task<GetUserResponse> ExecuteAsync(
        GetUserRequest request,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetDetailsByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException("Usuário não encontrado.");

        return new GetUserResponse(
            user.Id,
            user.Name,
            user.Email,
            user.Status,
            user.ProfileId,
            user.ProfileName,
            user.ProfileScope,
            user.ProfileIsFixed,
            user.CreatedAt,
            user.InvitedAt,
            user.InviteExpiresAt,
            user.InviteExpired,
            user.BrokerageMemberships
                .Select(membership => new UserMembershipResponse(
                    membership.Id,
                    membership.ScopeId,
                    membership.ScopeDocumentNumber,
                    membership.ScopeName,
                    membership.ProfileId,
                    membership.ProfileName,
                    membership.ProfileScope,
                    membership.ProfileIsFixed))
                .ToList(),
            user.PolicyHolderMemberships
                .Select(membership => new UserMembershipResponse(
                    membership.Id,
                    membership.ScopeId,
                    membership.ScopeDocumentNumber,
                    membership.ScopeName,
                    membership.ProfileId,
                    membership.ProfileName,
                    membership.ProfileScope,
                    membership.ProfileIsFixed))
                .ToList());
    }
}

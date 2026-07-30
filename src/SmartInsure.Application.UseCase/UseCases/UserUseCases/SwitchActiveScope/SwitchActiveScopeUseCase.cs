using SmartInsure.Application.UseCase.Services.Scopes;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.SwitchActiveScope.Interfaces;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.SwitchActiveScope.Requests;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.SwitchActiveScope.Responses;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.SwitchActiveScope;

/// <summary>
/// RN-064 — troca da Corretora ativa / do Tomador ativo. O Vínculo é conferido no servidor
/// (RN-064: operação só é permitida no Escopo ativo em que o Usuário tem Perfil) e o acesso é
/// reemitido com o novo Escopo (ADR-065). A troca não altera Vínculos nem Perfis.
/// </summary>
public sealed class SwitchActiveScopeUseCase(
    IUserRepository userRepository,
    IActiveScopeResolver activeScopeResolver,
    IAccessTokenIssuer accessTokenIssuer) : ISwitchActiveScopeUseCase
{
    public async Task<SwitchActiveScopeResponse> ExecuteAsync(
        SwitchActiveScopeRequest request,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByExternalIdentityAsync(
            request.ExternalIdentity, cancellationToken)
            ?? throw new NotFoundException("Usuário não encontrado.");

        // RN-005/RN-076: acesso é privilégio de Usuário Ativo — reemitir para Inativo daria
        // sobrevida à sessão de quem perdeu o acesso.
        if (user.Status != EUserStatus.Active)
        {
            throw new BusinessRuleException("Somente um usuário ativo troca de escopo.");
        }

        var activeScope = await activeScopeResolver.ResolveRequestedAsync(
            user.Id, request.BrokerageId, request.PolicyHolderId, cancellationToken);

        var accessToken = accessTokenIssuer.IssueFor(user, activeScope);

        return new SwitchActiveScopeResponse(
            accessToken.Token,
            accessToken.ExpiresAtUtc,
            activeScope.BrokerageId,
            activeScope.PolicyHolderId);
    }
}

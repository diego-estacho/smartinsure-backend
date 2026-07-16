using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.LogoutUser.Interfaces;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.LogoutUser.Requests;
using SmartInsure.Core.Abstractions.Services;

namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.LogoutUser;

/// <summary>
/// RN-006 — Encerramento de sessão: registra o acesso corrente na denylist até a
/// expiração natural dele. Idempotente; acesso sem identificador ou já expirado é no-op.
/// </summary>
public sealed class LogoutUserUseCase(
    IAccessTokenRevocationStore revocationStore) : ILogoutUserUseCase
{
    /// <summary>Executa o caso de uso de encerramento de sessão.</summary>
    public async Task<Unit> ExecuteAsync(
        LogoutUserRequest request,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(request.SessionTokenId)
            && request.SessionExpiresAtUtc is { } expiresAtUtc)
        {
            await revocationStore.RevokeAsync(
                request.SessionTokenId, expiresAtUtc, cancellationToken);
        }

        return Unit.Value;
    }
}

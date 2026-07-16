namespace SmartInsure.Core.Abstractions.Services;

/// <summary>
/// Denylist de acessos encerrados (RN-006): o encerramento registra o identificador do
/// token até a expiração natural dele; a validação de entrada consulta antes de aceitar.
/// </summary>
public interface IAccessTokenRevocationStore
{
    Task RevokeAsync(string tokenId, DateTime expiresAtUtc, CancellationToken cancellationToken);

    Task<bool> IsRevokedAsync(string tokenId, CancellationToken cancellationToken);
}

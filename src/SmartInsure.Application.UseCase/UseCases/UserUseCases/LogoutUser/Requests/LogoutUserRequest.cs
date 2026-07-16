namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.LogoutUser.Requests;

/// <summary>Dados de entrada do encerramento de sessão (RN-006), extraídos do acesso corrente.</summary>
/// <param name="SessionTokenId">Identificador único do acesso autenticado (jti).</param>
/// <param name="SessionExpiresAtUtc">Expiração do acesso autenticado, em UTC.</param>
public sealed record LogoutUserRequest(string? SessionTokenId, DateTime? SessionExpiresAtUtc);

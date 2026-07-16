namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.AuthenticateUser.Responses;

/// <summary>Dados de saída da autenticação (RN-005).</summary>
/// <param name="AccessToken">Token de acesso da plataforma.</param>
/// <param name="ExpiresAtUtc">Fim da validade do acesso, em UTC.</param>
public sealed record AuthenticateUserResponse(string AccessToken, DateTime ExpiresAtUtc);

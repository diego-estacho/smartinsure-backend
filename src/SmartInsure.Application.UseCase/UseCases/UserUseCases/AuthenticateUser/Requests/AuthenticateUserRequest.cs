namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.AuthenticateUser.Requests;

/// <summary>Dados de entrada para autenticação de usuário (RN-005).</summary>
/// <param name="Email">Endereço de e-mail do usuário.</param>
/// <param name="Password">Senha do usuário, validada no provedor de identidade.</param>
public sealed record AuthenticateUserRequest(string Email, string Password);

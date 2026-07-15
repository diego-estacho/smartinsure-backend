namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.CreateUser.Requests;

/// <summary>Dados de entrada para criar um novo usuário.</summary>
/// <param name="Name">Nome completo do usuário.</param>
/// <param name="Email">Endereço de e-mail único do usuário.</param>
public sealed record CreateUserRequest(string Name, string Email);

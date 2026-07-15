namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.CreateUser.Responses;

/// <summary>Dados de saída da criação de usuário.</summary>
/// <param name="Id">Identificador único do usuário criado.</param>
/// <param name="Name">Nome completo do usuário.</param>
/// <param name="Email">Endereço de e-mail do usuário.</param>
/// <param name="Status">Status atual do usuário (ex.: Pendente, Ativo).</param>
public sealed record CreateUserResponse(Guid Id, string Name, string Email, string Status);

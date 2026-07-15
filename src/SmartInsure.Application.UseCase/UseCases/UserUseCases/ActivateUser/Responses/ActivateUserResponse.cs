namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.ActivateUser.Responses;

/// <summary>Dados de saída da ativação de usuário.</summary>
/// <param name="Id">Identificador único do usuário ativado.</param>
/// <param name="Status">Status atual do usuário após ativação (ex.: Ativo).</param>
public sealed record ActivateUserResponse(Guid Id, string Status);

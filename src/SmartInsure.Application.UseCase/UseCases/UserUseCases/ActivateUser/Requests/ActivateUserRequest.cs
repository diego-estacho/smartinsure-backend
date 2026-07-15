namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.ActivateUser.Requests;

/// <summary>Dados de entrada para ativar um usuário.</summary>
/// <param name="ExternalIdentity">Identificador da identidade do usuário no provedor de identidade.</param>
public sealed record ActivateUserRequest(string ExternalIdentity);

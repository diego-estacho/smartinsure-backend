namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.SetUserProfile.Requests;

/// <summary>Dados de entrada para concessão/revogação de perfil.</summary>
/// <param name="UserId">Identificador único do usuário.</param>
/// <param name="Profile">Nome do perfil (null para revogação).</param>
public sealed record SetUserProfileRequest(Guid UserId, string? Profile);

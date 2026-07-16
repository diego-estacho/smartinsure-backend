namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.SetUserProfile.Responses;

/// <summary>Dados de saída da concessão/revogação de perfil.</summary>
/// <param name="Id">Identificador único do usuário.</param>
/// <param name="Profile">Perfil atual do usuário (null se revogado).</param>
public sealed record SetUserProfileResponse(Guid Id, string? Profile);

namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.ChangeUserScopeProfile.Responses;

/// <summary>RN-075: resultado da troca de Perfil no vínculo.</summary>
public sealed record ChangeUserScopeProfileResponse(
    Guid UserId, Guid ScopeId, Guid ProfileId, string ProfileName);

namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.ChangeUserScopeProfile.Requests;

/// <summary>
/// RN-075: troca o Perfil do Usuário dentro de um Escopo (Corretora ou Tomador). O Escopo é a
/// Person (Corretora/Tomador) do vínculo; o novo Perfil precisa ser do mesmo Escopo (RN-072).
/// </summary>
public sealed record ChangeUserScopeProfileRequest(Guid UserId, Guid ScopeId, Guid ProfileId);

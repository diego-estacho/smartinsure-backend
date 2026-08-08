namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.RequestPasswordReset.Requests;

/// <summary>RN-203: pedido de redefinição de senha de um Usuário Ativo (disparado pelo administrador).</summary>
public sealed record RequestPasswordResetRequest(Guid UserId);

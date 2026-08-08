namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.RequestPasswordReset.Responses;

/// <summary>RN-203: confirmação do envio — para quem o link de redefinição foi enviado.</summary>
public sealed record RequestPasswordResetResponse(Guid UserId, string Email);

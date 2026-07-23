namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.ResendInvitation.Responses;

/// <summary>RN-035: resposta do reenvio — novo convite gerado e enviado.</summary>
public sealed record ResendInvitationResponse(Guid UserId, string Email);

namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.ResendInvitation.Requests;

/// <summary>RN-035: reenvio do convite de primeiro acesso.</summary>
public sealed record ResendInvitationRequest(Guid UserId);

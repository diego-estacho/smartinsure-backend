namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.ResendInvitation.Requests;

/// <summary>RN-065: reenvio do convite de primeiro acesso.</summary>
public sealed record ResendInvitationRequest(Guid UserId);

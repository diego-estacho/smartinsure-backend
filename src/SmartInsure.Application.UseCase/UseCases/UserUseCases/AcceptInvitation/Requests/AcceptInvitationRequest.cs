namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.AcceptInvitation.Requests;

/// <summary>RN-065: dados do primeiro acesso — token do convite + senha escolhida.</summary>
public sealed record AcceptInvitationRequest(string Token, string Password);

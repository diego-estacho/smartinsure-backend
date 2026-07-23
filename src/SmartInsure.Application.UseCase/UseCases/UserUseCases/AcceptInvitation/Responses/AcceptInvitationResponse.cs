namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.AcceptInvitation.Responses;

/// <summary>RN-035: resposta do primeiro acesso — usuário agora está Ativo.</summary>
public sealed record AcceptInvitationResponse(Guid UserId, string Name, string Email, string Status);

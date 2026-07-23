namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.ChangeUserActivation.Requests;

/// <summary>RN-046: inativa (Activate=false) ou reativa (Activate=true) um Usuário.</summary>
public sealed record ChangeUserActivationRequest(Guid UserId, bool Activate);

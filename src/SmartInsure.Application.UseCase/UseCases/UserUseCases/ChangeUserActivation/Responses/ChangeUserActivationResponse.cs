namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.ChangeUserActivation.Responses;

/// <summary>RN-076: situação resultante do Usuário.</summary>
public sealed record ChangeUserActivationResponse(Guid Id, string Status);

namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.EditUser.Responses;

/// <summary>RN-202: resultado da edição (situação por nome estável, ADR-031).</summary>
public sealed record EditUserResponse(Guid Id, string Name, string Email, string Status);

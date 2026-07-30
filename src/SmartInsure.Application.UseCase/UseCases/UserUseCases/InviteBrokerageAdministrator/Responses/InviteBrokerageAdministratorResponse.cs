namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.InviteBrokerageAdministrator.Responses;

/// <summary>RN-066: resultado do convite de Corretor Administrador.</summary>
public sealed record InviteBrokerageAdministratorResponse(Guid Id, string Name, string Email, string Status);

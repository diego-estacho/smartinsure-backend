namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.InvitePolicyHolderAdministrator.Responses;

/// <summary>RN-068: resultado do convite de Tomador Administrador.</summary>
public sealed record InvitePolicyHolderAdministratorResponse(
    Guid Id,
    string Name,
    string Email,
    string Status,
    Guid PolicyHolderId);

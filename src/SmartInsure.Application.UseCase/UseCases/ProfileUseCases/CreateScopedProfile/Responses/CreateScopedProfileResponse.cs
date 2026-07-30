namespace SmartInsure.Application.UseCase.UseCases.ProfileUseCases.CreateScopedProfile.Responses;

/// <summary>RN-069/RN-070: Perfil customizado criado no Escopo do solicitante.</summary>
public sealed record CreateScopedProfileResponse(
    Guid Id,
    string Name,
    string Scope,
    Guid? BrokerageId,
    Guid? PolicyHolderId,
    int PermissionCount);

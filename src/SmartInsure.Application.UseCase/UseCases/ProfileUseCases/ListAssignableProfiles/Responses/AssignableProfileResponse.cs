namespace SmartInsure.Application.UseCase.UseCases.ProfileUseCases.ListAssignableProfiles.Responses;

/// <summary>RN-072: Perfil oferecido na criação de Usuário, com o Escopo em que será concedido.</summary>
public sealed record AssignableProfileResponse(
    Guid Id,
    string Name,
    string Scope,
    bool IsFixed,
    Guid? BrokerageId,
    Guid? PolicyHolderId);

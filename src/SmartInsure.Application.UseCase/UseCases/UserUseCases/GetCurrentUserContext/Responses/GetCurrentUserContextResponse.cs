namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.GetCurrentUserContext.Responses;

/// <summary>
/// RN-064 — quem está logado, onde pode operar e onde está operando agora. É o que alimenta
/// o seletor de Corretora/Tomador ativo da interface.
/// </summary>
public sealed record GetCurrentUserContextResponse(
    Guid Id,
    string Name,
    string Email,
    string Status,
    string? SystemProfileName,
    Guid? ActiveBrokerageId,
    Guid? ActivePolicyHolderId,
    IReadOnlyList<UserScopeResponse> Brokerages,
    IReadOnlyList<UserScopeResponse> PolicyHolders);

/// <summary>Escopo em que o Usuário tem Vínculo, com o Perfil que ele tem ali (RN-062/RN-064).</summary>
public sealed record UserScopeResponse(
    Guid Id,
    string DocumentNumber,
    string Name,
    string ProfileName,
    bool IsActive);

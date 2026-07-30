namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.SwitchActiveScope.Responses;

/// <summary>
/// RN-064/ADR-065: a troca de Escopo reemite o acesso — o novo token carrega o Escopo ativo
/// escolhido. O cliente substitui o acesso anterior por este.
/// </summary>
public sealed record SwitchActiveScopeResponse(
    string AccessToken,
    DateTime ExpiresAtUtc,
    Guid? ActiveBrokerageId,
    Guid? ActivePolicyHolderId);

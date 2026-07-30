namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.SwitchActiveScope.Requests;

/// <summary>
/// RN-064: troca do Escopo ativo. A identidade vem do acesso; o cliente escolhe apenas
/// para qual Corretora/Tomador quer mudar — e só entre aqueles em que tem Vínculo.
/// Identificador nulo significa sair daquele Escopo.
/// </summary>
public sealed record SwitchActiveScopeRequest(
    string ExternalIdentity,
    Guid? BrokerageId,
    Guid? PolicyHolderId);

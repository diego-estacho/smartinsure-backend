namespace SmartInsure.Core.Abstractions.Services;

/// <summary>
/// Contrato do provedor de identidade (glossário) — implementado na camada de
/// integração (Casdoor). RN-001: identidade criada com senha inicial padrão e troca
/// obrigatória; RN-002: a conclusão da troca é consultada aqui.
/// </summary>
public interface IIdentityProvider
{
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken);

    /// <summary>Cria a identidade com senha inicial padrão e retorna o identificador dela.</summary>
    Task<string> CreateIdentityAsync(string name, string email, CancellationToken cancellationToken);

    /// <summary>Compensação da RN-001: desfaz identidade recém-criada quando a criação local falha.</summary>
    Task RemoveIdentityAsync(string externalIdentity, CancellationToken cancellationToken);

    /// <summary>RN-002: indica se a senha inicial padrão ainda está pendente de troca.</summary>
    Task<bool> IsInitialPasswordPendingAsync(string externalIdentity, CancellationToken cancellationToken);

    /// <summary>
    /// RN-005: valida e-mail e senha exclusivamente no provedor de identidade — a plataforma
    /// não guarda nem valida senhas. Provedor fora do ar lança
    /// <see cref="Exceptions.IdentityProviderUnavailableException"/>.
    /// </summary>
    Task<bool> ValidateCredentialsAsync(string email, string password, CancellationToken cancellationToken);
}

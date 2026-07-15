using System.ComponentModel.DataAnnotations;

namespace SmartInsure.Integration.Casdoor.Options;

/// <summary>
/// Configuração do provedor de identidade (seção SSO). Secret e DefaultPassword vivem
/// em appsettings.*.Local.json / secret store — nunca versionados (SECURITY.md).
/// </summary>
public sealed class CasdoorOptions
{
    public const string SectionName = "SSO";

    [Required]
    public string Domain { get; init; } = string.Empty;

    [Required]
    public string ClientId { get; init; } = string.Empty;

    [Required]
    public string Secret { get; init; } = string.Empty;

    [Required]
    public string OrganizationName { get; init; } = string.Empty;

    [Required]
    public string AppName { get; init; } = string.Empty;

    /// <summary>Senha inicial padrão da RN-001, com troca obrigatória no primeiro acesso.</summary>
    [Required]
    public string DefaultPassword { get; init; } = string.Empty;

    /// <summary>
    /// Prefixo de ambiente do username no Casdoor (ex.: dev_insp). Nome da chave mantém a
    /// grafia da configuração legada compartilhada com o InsurePoint.
    /// </summary>
    [Required]
    public string EnviromentUserCasdoor { get; init; } = string.Empty;
}

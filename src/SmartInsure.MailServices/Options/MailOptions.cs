using System.ComponentModel.DataAnnotations;

namespace SmartInsure.MailServices.Options;

/// <summary>
/// Configuração SMTP (ADR-048, ADR-053) — classe única, seção "MailSettings".
/// User e Password vêm de variável de ambiente ou appsettings.{Ambiente}.Local.json —
/// nunca versionados (SECURITY.md, ADR-054).
/// </summary>
public sealed class MailOptions
{
    public const string SectionName = "MailSettings";

    [Required]
    public string Host { get; init; } = string.Empty;

    [Range(1, 65535)]
    public int Port { get; init; } = 587;

    [Required]
    public string User { get; init; } = string.Empty;

    [Required]
    public string Password { get; init; } = string.Empty;

    [Required]
    public string DisplayName { get; init; } = string.Empty;

    [Required]
    [EmailAddress]
    public string From { get; init; } = string.Empty;

    /// <summary>true → TLS implícito na conexão; false → STARTTLS quando disponível.</summary>
    public bool UseSsl { get; init; }
}

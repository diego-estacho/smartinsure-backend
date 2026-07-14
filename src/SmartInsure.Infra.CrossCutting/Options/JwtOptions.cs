using System.ComponentModel.DataAnnotations;

namespace SmartInsure.Infra.CrossCutting.Options;

/// <summary>
/// Validação do JWT Bearer (ADR-015): chave simétrica compartilhada com o IdP (Casdoor).
/// O SigningKey vem de variável de ambiente ou appsettings.{Ambiente}.Local.json — nunca versionado (ADR-054).
/// </summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required]
    public string Issuer { get; init; } = string.Empty;

    [Required]
    public string Audience { get; init; } = string.Empty;

    [Required]
    public string SigningKey { get; init; } = string.Empty;

    public string RoleClaimType { get; init; } = "role";
}

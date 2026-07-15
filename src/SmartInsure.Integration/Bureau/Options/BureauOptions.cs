using System.ComponentModel.DataAnnotations;

namespace SmartInsure.Integration.Bureau.Options;

/// <summary>
/// Configuração do gateway de Birô (seção BureauApi). Password vive em
/// appsettings.*.Local.json / secret store — nunca versionada (SECURITY.md).
/// </summary>
public sealed class BureauOptions
{
    public const string SectionName = "BureauApi";

    [Required]
    public string BaseUrl { get; init; } = string.Empty;

    [Required]
    public string UserName { get; init; } = string.Empty;

    [Required]
    public string Password { get; init; } = string.Empty;

    /// <summary>Identificador da companhia no gateway, enviado no header InsuranceCompanyId.</summary>
    [Required]
    public string InsuranceCompanyId { get; init; } = string.Empty;

    /// <summary>Nome do produto no gateway, enviado no header Product.</summary>
    [Required]
    public string Product { get; init; } = string.Empty;
}

using System.ComponentModel.DataAnnotations;

namespace SmartInsure.Integration.CalculationEngines.Options;

/// <summary>
/// Configuração do motor PlugV2 (seção PlugV2Api). Credenciais do vínculo vivem na
/// Habilitação de Seguradora (RN-022); aqui fica só o endereço do gateway. Segredos, se
/// surgirem, vivem em appsettings.*.Local.json / secret store — nunca versionados (SECURITY.md).
/// </summary>
public sealed class PlugV2Options
{
    public const string SectionName = "PlugV2Api";

    [Required]
    public string BaseUrl { get; init; } = string.Empty;
}

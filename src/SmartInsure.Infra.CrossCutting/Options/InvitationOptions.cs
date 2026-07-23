using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;

namespace SmartInsure.Infra.CrossCutting.Options;

/// <summary>Configuração de Convites (RN-035): URL da aplicação para links do convite e validade.</summary>
public sealed class InvitationOptions : IValidatableObject
{
    public const string SectionName = "Invitations";

    /// <summary>URL base da aplicação pública (ex.: https://app.smartinsure.com) para compor links.</summary>
    public string AppBaseUrl { get; set; } = null!;

    /// <summary>Validade do link em dias (padrão 7, RN-035).</summary>
    public int LinkExpiryDays { get; set; } = 7;

    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (string.IsNullOrWhiteSpace(AppBaseUrl))
        {
            yield return new ValidationResult(
                $"{nameof(AppBaseUrl)} é obrigatório",
                [nameof(AppBaseUrl)]);
        }

        if (LinkExpiryDays <= 0)
        {
            yield return new ValidationResult(
                $"{nameof(LinkExpiryDays)} deve ser > 0",
                [nameof(LinkExpiryDays)]);
        }
    }
}

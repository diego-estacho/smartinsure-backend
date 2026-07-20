using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Core.Entities;

/// <summary>
/// Seguradora do catálogo (RN-007..RN-010): mantida somente pelo Administrador do Sistema;
/// nunca excluída — Inativa significa fora de operação (RN-009).
/// </summary>
public sealed class Insurer : EntityBase
{
    private Insurer()
    {
    }

    public string Cnpj { get; private set; } = string.Empty;

    public string CorporateName { get; private set; } = string.Empty;

    public string? TradeName { get; private set; }

    public string? LogoUrl { get; private set; }

    /// <summary>
    /// Identificador da Seguradora no sistema de origem do Motor de Cálculo
    /// (ex.: InsuranceUniqueId no PlugV2). Opcional — nem toda seguradora opera via motor externo.
    /// </summary>
    public Guid? ReferenceExternalId { get; private set; }

    public EInsurerStatus Status { get; private set; }

    public static Insurer Create(
        string cnpj,
        string corporateName,
        string? tradeName,
        string? logoUrl,
        EInsurerStatus initialStatus,
        Guid? referenceExternalId = null)
    {
        var insurer = new Insurer { Status = initialStatus, ReferenceExternalId = referenceExternalId };
        insurer.SetDetails(cnpj, corporateName, tradeName, logoUrl);
        return insurer;
    }

    /// <summary>RN-008: alteração cadastral mantém as exigências do cadastro; situação não muda aqui.</summary>
    public void UpdateDetails(
        string cnpj, string corporateName, string? tradeName, string? logoUrl, Guid? referenceExternalId = null)
    {
        SetDetails(cnpj, corporateName, tradeName, logoUrl);
        ReferenceExternalId = referenceExternalId;
    }

    /// <summary>RN-009: Inativa → Ativa; ativar quem já está Ativa é conflito de estado.</summary>
    public void Activate()
    {
        if (Status == EInsurerStatus.Active)
        {
            throw new ConflictException("A seguradora já está ativa.");
        }

        Status = EInsurerStatus.Active;
    }

    /// <summary>RN-009: Ativa → Inativa (fora de operação, nunca excluída).</summary>
    public void Deactivate()
    {
        if (Status == EInsurerStatus.Inactive)
        {
            throw new ConflictException("A seguradora já está inativa.");
        }

        Status = EInsurerStatus.Inactive;
    }

    private void SetDetails(string cnpj, string corporateName, string? tradeName, string? logoUrl)
    {
        Cnpj = new string([.. cnpj.Where(char.IsDigit)]);
        CorporateName = corporateName.Trim();
        TradeName = string.IsNullOrWhiteSpace(tradeName) ? null : tradeName.Trim();
        LogoUrl = string.IsNullOrWhiteSpace(logoUrl) ? null : logoUrl.Trim();
    }
}

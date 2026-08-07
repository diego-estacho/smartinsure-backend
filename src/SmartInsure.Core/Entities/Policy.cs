using SmartInsure.Core.Exceptions;

namespace SmartInsure.Core.Entities;

/// <summary>
/// Apólice (RN-514): registro da emissão de uma Cotação. Nasce quando a emissão é **solicitada** e
/// aceita pela Seguradora — a plataforma não afirma emissão confirmada nesta fase, então número da
/// apólice, arquivo e boletos não são registrados aqui (vêm da confirmação, demanda própria).
/// Guarda os valores vigentes no momento da emissão, inclusive os recalculados por ajuste de taxa
/// (RN-504), a forma de pagamento escolhida (RN-505), o endereço do Segurado enviado (RN-503), o
/// aceite do Termo (RN-506) e quem solicitou. Uma por Cotação (RN-507).
/// </summary>
public sealed class Policy : EntityBase
{
    private Policy()
    {
    }

    public Guid QuotationGroupId { get; private set; }

    public Guid QuotationId { get; private set; }

    /// <summary>Referência da apólice devolvida pela Seguradora no pedido de emissão.</summary>
    public string PolicyExternalId { get; private set; } = string.Empty;

    /// <summary>Número da proposta na Seguradora — é por ele que o corretor identifica a oferta.</summary>
    public string? ProposalNumber { get; private set; }

    public decimal? Premium { get; private set; }

    public decimal? Tax { get; private set; }

    public decimal? CommissionPercentage { get; private set; }

    public decimal? CommissionValue { get; private set; }

    /// <summary>RN-505: parcelamento escolhido entre os informados pela Seguradora.</summary>
    public int InstallmentNumber { get; private set; }

    /// <summary>RN-505: dias para o vencimento da primeira parcela, escolhido entre os informados.</summary>
    public int GracePeriodInDays { get; private set; }

    /// <summary>RN-506: aceite do Termo que autorizou esta emissão.</summary>
    public Guid TermAcceptanceId { get; private set; }

    /// <summary>Usuário que solicitou a emissão.</summary>
    public Guid RequestedByUserId { get; private set; }

    public DateTime RequestedAt { get; private set; }

    /// <summary>RN-503: snapshot do endereço do Segurado efetivamente enviado à Seguradora.</summary>
    public string? InsuredAddressZipCode { get; private set; }

    public string? InsuredAddressStreet { get; private set; }

    public string? InsuredAddressNumber { get; private set; }

    public string? InsuredAddressComplement { get; private set; }

    public string? InsuredAddressNeighborhood { get; private set; }

    public string? InsuredAddressCity { get; private set; }

    public string? InsuredAddressState { get; private set; }

    /// <summary>
    /// RN-514: registra a emissão solicitada da Cotação escolhida. O endereço é copiado da réplica da
    /// oferta (RN-503) — snapshot do que foi enviado, imune a alteração posterior.
    /// </summary>
    public static Policy RequestIssuance(
        Quotation quotation,
        QuotationGroup group,
        string policyExternalId,
        string? proposalNumber,
        int installmentNumber,
        int gracePeriodInDays,
        Guid termAcceptanceId,
        Guid requestedByUserId,
        DateTime requestedAt)
    {
        if (string.IsNullOrWhiteSpace(policyExternalId))
        {
            throw new BusinessRuleException("A Seguradora não devolveu a referência da apólice.");
        }

        var address = group.InsuredAddress;

        return new Policy
        {
            QuotationGroupId = group.Id,
            QuotationId = quotation.Id,
            PolicyExternalId = policyExternalId.Trim(),
            ProposalNumber = string.IsNullOrWhiteSpace(proposalNumber) ? null : proposalNumber.Trim(),
            Premium = quotation.Premium,
            Tax = quotation.Tax,
            CommissionPercentage = quotation.CommissionPercentage,
            CommissionValue = quotation.CommissionValue,
            InstallmentNumber = installmentNumber,
            GracePeriodInDays = gracePeriodInDays,
            TermAcceptanceId = termAcceptanceId,
            RequestedByUserId = requestedByUserId,
            RequestedAt = requestedAt,
            InsuredAddressZipCode = address?.ZipCode,
            InsuredAddressStreet = address?.Street,
            InsuredAddressNumber = address?.Number,
            InsuredAddressComplement = address?.Complement,
            InsuredAddressNeighborhood = address?.Neighborhood,
            InsuredAddressCity = address?.City,
            InsuredAddressState = address?.State,
        };
    }
}

using SmartInsure.Core.Enumerators;

namespace SmartInsure.Core.Entities;

/// <summary>
/// Resultado individual de uma Consulta de Crédito para uma Seguradora (RN-029/RN-030).
/// Status Available: limites e taxas por modalidade preenchidos. Status Unavailable: apenas motivo.
/// Imutável após criação — nunca editado ou excluído (RN-031).
/// </summary>
public sealed class CreditInquiryResult : EntityBase
{
    private CreditInquiryResult()
    {
    }

    public Guid CreditInquiryId { get; private set; }

    public Guid InsurerId { get; private set; }

    public ECreditInquiryResultStatus Status { get; private set; }

    public string? FailureReason { get; private set; }

    public decimal? TraditionalLimit { get; private set; }

    public decimal? TraditionalRate { get; private set; }

    public decimal? JudicialLimit { get; private set; }

    public decimal? JudicialRate { get; private set; }

    public decimal? JudicialFiscalRate { get; private set; }

    public decimal? FinancialLimit { get; private set; }

    public decimal? FinancialRate { get; private set; }

    public DateTime? LimitValidUntil { get; private set; }

    /// <summary>RN-030: resultado disponível com todos os limites e taxas da seguradora.</summary>
    public static CreditInquiryResult Available(
        Guid creditInquiryId,
        Guid insurerId,
        decimal? traditionalLimit,
        decimal? traditionalRate,
        decimal? judicialLimit,
        decimal? judicialRate,
        decimal? judicialFiscalRate,
        decimal? financialLimit,
        decimal? financialRate,
        DateTime? limitValidUntil)
        => new()
        {
            CreditInquiryId = creditInquiryId,
            InsurerId = insurerId,
            Status = ECreditInquiryResultStatus.Available,
            TraditionalLimit = traditionalLimit,
            TraditionalRate = traditionalRate,
            JudicialLimit = judicialLimit,
            JudicialRate = judicialRate,
            JudicialFiscalRate = judicialFiscalRate,
            FinancialLimit = financialLimit,
            FinancialRate = financialRate,
            LimitValidUntil = limitValidUntil,
        };

    /// <summary>RN-030: resultado indisponível com motivo da falha.</summary>
    public static CreditInquiryResult Unavailable(
        Guid creditInquiryId,
        Guid insurerId,
        string failureReason)
        => new()
        {
            CreditInquiryId = creditInquiryId,
            InsurerId = insurerId,
            Status = ECreditInquiryResultStatus.Unavailable,
            FailureReason = failureReason,
        };
}

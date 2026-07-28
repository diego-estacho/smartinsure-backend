using SmartInsure.Core.Enumerators;

namespace SmartInsure.Core.Entities;

/// <summary>
/// Cotação (RN-057, RN-058, RN-059, ADR-064): o retorno de UMA Seguradora para um Grupo de Cotação.
/// Nasce <see cref="EQuotationProcessingStatus.Requested"/> antes do fan-out (persistida antes de enfileirar,
/// ADR-050) e é preenchida quando a Seguradora responde — com a classificação estável do resultado, a
/// esteira (quando Análise), os motivos (quando Indisponível/Recusado), o prêmio/condições (quando aplicável)
/// e o veredito de Contragarantia (CCG). Agregado próprio: cada Cotação é persistida de forma independente.
/// </summary>
public sealed class Quotation : EntityBase
{
    private readonly List<QuotationReason> _reasons = [];

    private Quotation()
    {
    }

    public Guid QuotationGroupId { get; private set; }

    /// <summary>Corretora que solicitou a cotação — permite ao consumidor/reconciliador resolver a Habilitação.</summary>
    public Guid BrokerageId { get; private set; }

    public Guid InsurerId { get; private set; }

    public EQuotationProcessingStatus ProcessingStatus { get; private set; }

    /// <summary>Classificação estável do resultado (preenchida quando obtida).</summary>
    public EQuotationResult? Result { get; private set; }

    /// <summary>Esteira da análise, quando <see cref="Result"/> é Análise (RN-058).</summary>
    public EAnalysisTrack? AnalysisTrack { get; private set; }

    public decimal? Premium { get; private set; }

    public decimal? CommissionPercentage { get; private set; }

    public decimal? CommissionValue { get; private set; }

    public decimal? Tax { get; private set; }

    public decimal? AvailableLimit { get; private set; }

    /// <summary>Identificador externo da proposta na Seguradora, para as etapas seguintes (followup/emissão).</summary>
    public string? ProposalExternalId { get; private set; }

    public string? ProposalNumber { get; private set; }

    /// <summary>RN-058/ADR-064: a Seguradora exige Contragarantia (CCG) para emitir. Ortogonal à classificação.</summary>
    public bool RequiresCcg { get; private set; }

    public decimal? CcgMaxLimitWithoutNeed { get; private set; }

    public bool CcgSigned { get; private set; }

    /// <summary>Instante em que a Seguradora respondeu (base da validade — RN-061).</summary>
    public DateTime? ObtainedAt { get; private set; }

    public IReadOnlyCollection<QuotationReason> Reasons => _reasons.AsReadOnly();

    /// <summary>
    /// RN-059: a Cotação é seguível quando Automática ou em Análise de Subscrição. A exigência de CCG
    /// NÃO bloqueia a seguibilidade (o erro, se houver, é na emissão — ADR-064).
    /// </summary>
    public bool IsFollowable =>
        ProcessingStatus == EQuotationProcessingStatus.Obtained
        && (Result == EQuotationResult.Automatic
            || (Result == EQuotationResult.Analysis && AnalysisTrack == EAnalysisTrack.Underwriting));

    /// <summary>RN-057/ADR-050: cria a Cotação em Requested (persistida antes de enfileirar o fan-out).</summary>
    public static Quotation Request(Guid quotationGroupId, Guid brokerageId, Guid insurerId)
        => new()
        {
            QuotationGroupId = quotationGroupId,
            BrokerageId = brokerageId,
            InsurerId = insurerId,
            ProcessingStatus = EQuotationProcessingStatus.Requested,
        };

    /// <summary>
    /// RN-058: registra o resultado obtido da Seguradora (classificação + esteira + prêmio/condições + CCG).
    /// Uma Cotação sem prêmio aplicável (Análise/Indisponível/Não-reconhecido) não guarda prêmio.
    /// </summary>
    public void MarkObtained(
        EQuotationResult result,
        EAnalysisTrack? analysisTrack,
        decimal? premium,
        decimal? commissionPercentage,
        decimal? commissionValue,
        decimal? tax,
        decimal? availableLimit,
        string? proposalExternalId,
        string? proposalNumber,
        bool requiresCcg,
        decimal? ccgMaxLimitWithoutNeed,
        bool ccgSigned,
        IEnumerable<string> reasons)
    {
        ProcessingStatus = EQuotationProcessingStatus.Obtained;
        Result = result;
        AnalysisTrack = result == EQuotationResult.Analysis ? analysisTrack : null;

        var hasApplicablePremium = result == EQuotationResult.Automatic;
        Premium = hasApplicablePremium ? premium : null;
        CommissionPercentage = hasApplicablePremium ? commissionPercentage : null;
        CommissionValue = hasApplicablePremium ? commissionValue : null;
        Tax = hasApplicablePremium ? tax : null;
        AvailableLimit = availableLimit;

        ProposalExternalId = proposalExternalId;
        ProposalNumber = proposalNumber;

        RequiresCcg = requiresCcg;
        CcgMaxLimitWithoutNeed = ccgMaxLimitWithoutNeed;
        CcgSigned = ccgSigned;

        ObtainedAt = DateTime.UtcNow;

        _reasons.Clear();
        foreach (var reason in reasons.Where(r => !string.IsNullOrWhiteSpace(r)))
        {
            AddReason(reason);
        }
    }

    /// <summary>RN-057: falha/indisponibilidade isolada da Seguradora (não derruba as demais) com motivo.</summary>
    public void MarkFailed(string reason)
    {
        ProcessingStatus = EQuotationProcessingStatus.Failed;
        Result = EQuotationResult.Unavailable;
        AnalysisTrack = null;
        Premium = null;
        CommissionPercentage = null;
        CommissionValue = null;
        Tax = null;
        ObtainedAt = DateTime.UtcNow;

        _reasons.Clear();
        if (!string.IsNullOrWhiteSpace(reason))
        {
            AddReason(reason);
        }
    }

    private void AddReason(string text)
    {
        var quotationReason = QuotationReason.Create(text);
        quotationReason.AttachTo(Id);
        _reasons.Add(quotationReason);
    }
}

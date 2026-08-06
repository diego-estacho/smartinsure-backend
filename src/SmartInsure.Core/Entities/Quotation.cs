using System.Text.Json;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Core.Entities;

/// <summary>
/// Cotação (RN-057/RN-058/RN-059): o resultado de uma Seguradora dentro de um Grupo de Cotação — uma
/// por Seguradora. Nasce Requested no fan-out e passa a Obtained/Failed conforme a Seguradora responde
/// (RN-057). Carrega a classificação estável (Result) + esteira/motivos/prêmio/CCG (RN-058, ADR-064) e
/// a minuta capturada quando selecionada (RN-079). A tradução do status do provedor vive na ACL, não
/// aqui (ADR-064); esta entidade guarda o resultado já classificado e garante os invariantes.
/// </summary>
public sealed class Quotation : EntityBase
{
    private readonly List<QuotationReason> _reasons = [];

    private readonly List<QuotationAdditionalCoverage> _additionalCoverages = [];

    private Quotation()
    {
    }

    public Guid QuotationGroupId { get; private set; }

    public Guid InsurerId { get; private set; }

    public EQuotationProcessingStatus ProcessingStatus { get; private set; }

    /// <summary>Classificação estável do resultado (null enquanto Requested). RN-058/ADR-064.</summary>
    public EQuotationResult? Result { get; private set; }

    /// <summary>Esteira específica quando Result = Analysis (RN-058).</summary>
    public EAnalysisTrack? AnalysisTrack { get; private set; }

    /// <summary>Prêmio — apenas quando Automatic (RN-058).</summary>
    public decimal? Premium { get; private set; }

    public decimal? CommissionPercentage { get; private set; }

    public decimal? CommissionValue { get; private set; }

    public decimal? Tax { get; private set; }

    public decimal? AvailableLimit { get; private set; }

    public string? ProposalExternalId { get; private set; }

    public string? ProposalNumber { get; private set; }

    /// <summary>Veredito de CCG, ortogonal à classificação (ADR-064): não bloqueia a seguibilidade.</summary>
    public bool RequiresCcg { get; private set; }

    public decimal? CcgMaxLimitWithoutNeed { get; private set; }

    public bool CcgSigned { get; private set; }

    /// <summary>Tags da minuta preenchidas (JSON) — capturadas na Cotação selecionada (RN-079).</summary>
    public string? MinutaTagsJson { get; private set; }

    /// <summary>Cláusulas particulares marcadas (JSON) — capturadas na Cotação selecionada (RN-079).</summary>
    public string? MinutaClausesJson { get; private set; }

    /// <summary>
    /// RN-512: Habilitação de Seguradora que obteve esta Cotação. A emissão usa esta mesma Habilitação —
    /// não a resolve de novo —, então inativá-la depois de cotar não impede emitir a oferta já cotada.
    /// </summary>
    public Guid? BrokerageInsurerEnablementId { get; private set; }

    /// <summary>Opções de parcelamento informadas pela Seguradora (JSON) — RN-505.</summary>
    public string? InstallmentOptionsJson { get; private set; }

    /// <summary>Dias possíveis de vencimento da primeira parcela (JSON) — RN-505.</summary>
    public string? PossibleGracePeriodsInDaysJson { get; private set; }

    /// <summary>Documentos exigidos pela Seguradora (JSON) — RN-510.</summary>
    public string? RequiredDocumentsJson { get; private set; }

    /// <summary>Instante em que a Seguradora respondeu (ou a falha foi registrada) — RN-057.</summary>
    public DateTime? ObtainedAt { get; private set; }

    /// <summary>
    /// Instante em que o consumidor começou a processar esta solicitação (lease do fan-out, ADR-050):
    /// carimbado antes de acionar o provedor, para o reconciliador distinguir uma Cotação em voo de uma
    /// órfã e não reenfileirar (duplicando a proposta) o que ainda está sendo obtido. Nulo enquanto na fila.
    /// </summary>
    public DateTime? ProcessingStartedAt { get; private set; }

    /// <summary>Motivos de indisponibilidade/recusa (RN-056/RN-058), do provedor ou locais.</summary>
    public IReadOnlyCollection<QuotationReason> Reasons => _reasons.AsReadOnly();

    /// <summary>RN-105/RN-106: situação das Coberturas Adicionais escolhidas nesta Cotação.</summary>
    public IReadOnlyCollection<QuotationAdditionalCoverage> AdditionalCoverages
        => _additionalCoverages.AsReadOnly();

    /// <summary>
    /// RN-106: registra a situação de cada Cobertura Adicional escolhida. Chamado ANTES de acionar a
    /// Seguradora, para que o registro exista mesmo quando a Cotação vira Indisponível (RN-058) ou
    /// falha na integração (RN-057). Substitui o registro anterior (recálculo).
    /// </summary>
    public void RecordAdditionalCoverages(IEnumerable<ResolvedAdditionalCoverage> resolved)
    {
        _additionalCoverages.Clear();

        foreach (var item in resolved)
        {
            _additionalCoverages.Add(QuotationAdditionalCoverage.Create(Id, item));
        }
    }

    /// <summary>RN-057: o fan-out materializa uma Cotação Requested por Seguradora antes de solicitar.</summary>
    public static Quotation Requested(Guid quotationGroupId, Guid insurerId)
        => new()
        {
            QuotationGroupId = quotationGroupId,
            InsurerId = insurerId,
            ProcessingStatus = EQuotationProcessingStatus.Requested,
        };

    /// <summary>
    /// RN-056: no modo Specific, uma Seguradora habilitada NÃO selecionada nasce indisponível com
    /// motivo local, sem ser cotada (não vai ao provedor).
    /// </summary>
    public static Quotation UnavailableLocal(Guid quotationGroupId, Guid insurerId, string reasonText)
    {
        var quotation = new Quotation
        {
            QuotationGroupId = quotationGroupId,
            InsurerId = insurerId,
            ProcessingStatus = EQuotationProcessingStatus.Obtained,
            Result = EQuotationResult.Unavailable,
        };

        quotation._reasons.Add(QuotationReason.Create(quotation.Id, reasonText, EQuotationReasonSource.Local));

        return quotation;
    }

    /// <summary>
    /// RN-057/ADR-050: marca o início do processamento (lease) antes de acionar o provedor — o reconciliador
    /// só reenfileira quando o lease expira, evitando duplicar a proposta de uma solicitação ainda em voo.
    /// </summary>
    public void BeginProcessing(DateTime startedAt) => ProcessingStartedAt = startedAt;

    /// <summary>
    /// RN-057/RN-058: a Seguradora respondeu — grava o resultado já classificado (vindo da ACL) com os
    /// invariantes do ADR-064: prêmio só em Automatic; Analysis exige esteira; Unavailable exige motivo;
    /// Unrecognized sem prêmio/esteira. Sobrescreve o estado Requested (idempotente por Seguradora).
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
        IEnumerable<string> reasonTexts,
        DateTime obtainedAt)
    {
        var reasons = (reasonTexts ?? []).Where(text => !string.IsNullOrWhiteSpace(text)).ToList();

        if (result == EQuotationResult.Analysis && analysisTrack is null)
        {
            throw new InvalidOperationException("Cotação em Análise exige a esteira específica (RN-058).");
        }

        if (result != EQuotationResult.Analysis && analysisTrack is not null)
        {
            throw new InvalidOperationException("Esteira só se aplica a Cotação em Análise (RN-058).");
        }

        if (result != EQuotationResult.ReadyForEmission && premium is not null)
        {
            throw new InvalidOperationException("Prêmio só é aplicável a Cotação Pronta para emissão (RN-058).");
        }

        if (result == EQuotationResult.Unavailable && reasons.Count == 0)
        {
            throw new InvalidOperationException("Cotação Indisponível exige ao menos um motivo (RN-058).");
        }

        ProcessingStatus = EQuotationProcessingStatus.Obtained;
        Result = result;
        AnalysisTrack = analysisTrack;
        Premium = premium;
        CommissionPercentage = commissionPercentage;
        CommissionValue = commissionValue;
        Tax = tax;
        AvailableLimit = availableLimit;
        ProposalExternalId = proposalExternalId;
        ProposalNumber = proposalNumber;
        RequiresCcg = requiresCcg;
        CcgMaxLimitWithoutNeed = ccgMaxLimitWithoutNeed;
        CcgSigned = ccgSigned;
        ObtainedAt = obtainedAt;

        _reasons.Clear();
        foreach (var text in reasons)
        {
            _reasons.Add(QuotationReason.Create(Id, text, EQuotationReasonSource.Provider));
        }
    }

    /// <summary>
    /// RN-057: falha/timeout ao obter (sem resposta utilizável do provedor) → Indisponível com motivo
    /// técnico local. Não há retry automático (o cotar cria proposta — a re-solicitação é manual).
    /// </summary>
    public void MarkFailed(string reasonText, DateTime failedAt)
    {
        ProcessingStatus = EQuotationProcessingStatus.Failed;
        Result = EQuotationResult.Unavailable;
        AnalysisTrack = null;
        Premium = null;
        ObtainedAt = failedAt;

        _reasons.Clear();
        _reasons.Add(QuotationReason.Create(
            Id,
            string.IsNullOrWhiteSpace(reasonText) ? "Falha técnica ao obter a Cotação." : reasonText,
            EQuotationReasonSource.Local));
    }

    /// <summary>
    /// RN-505/RN-510: registra o que a Seguradora informou nesta Cotação sobre pagamento e documentos —
    /// opções de parcelamento, dias possíveis de vencimento da primeira parcela e documentos exigidos.
    /// A etapa de emissão escolhe **dentro** dessas listas, sem acionar o provedor de novo. Coleção vazia
    /// significa "Seguradora não informou" e é registrada como ausência, nunca como opção inventada.
    /// </summary>
    public void SetProviderOptions(
        IEnumerable<QuotationInstallmentOption> installmentOptions,
        IEnumerable<int> possibleGracePeriodsInDays,
        IEnumerable<QuotationRequiredDocument> requiredDocuments)
    {
        // Serializado porque é dado do provedor, só leitura para a plataforma — mesma escolha da minuta.
        InstallmentOptionsJson = SerializeOrNull(installmentOptions?.ToList());
        PossibleGracePeriodsInDaysJson = SerializeOrNull(possibleGracePeriodsInDays?.ToList());
        RequiredDocumentsJson = SerializeOrNull(requiredDocuments?.ToList());
    }

    /// <summary>
    /// RN-504: aplica os valores recalculados pela Seguradora após o ajuste da taxa. Prêmio, comissão e
    /// opções de parcelamento passam a ser os devolvidos por ela — a plataforma não calcula dinheiro
    /// (ADR-004) e as opções antigas não sobrevivem, porque se referiam a outro prêmio. Só faz sentido em
    /// Cotação Pronta para emissão: nas outras classificações não há prêmio a recalcular (RN-058).
    /// </summary>
    public void ApplyFinancialData(
        decimal? premium,
        decimal? tax,
        decimal? commissionPercentage,
        decimal? commissionValue,
        IEnumerable<QuotationInstallmentOption> installmentOptions,
        IEnumerable<int> possibleGracePeriodsInDays)
    {
        if (Result != EQuotationResult.ReadyForEmission)
        {
            throw new BusinessRuleException(
                "Só Cotação Pronta para emissão tem valores a recalcular (RN-058/RN-504).");
        }

        Premium = premium;
        Tax = tax;
        CommissionPercentage = commissionPercentage;
        CommissionValue = commissionValue;

        SetProviderOptions(installmentOptions, possibleGracePeriodsInDays, ReadRequiredDocuments());
    }

    /// <summary>
    /// RN-512: guarda a Habilitação de Seguradora usada para obter esta Cotação, no momento em que ela
    /// é resolvida no fan-out — é por ela que a emissão aciona a Seguradora depois.
    /// </summary>
    public void SetEnablement(Guid brokerageInsurerEnablementId)
        => BrokerageInsurerEnablementId = brokerageInsurerEnablementId;

    /// <summary>RN-505: opções de parcelamento informadas pela Seguradora.</summary>
    public IReadOnlyList<QuotationInstallmentOption> ReadInstallmentOptions()
        => Deserialize<QuotationInstallmentOption>(InstallmentOptionsJson);

    /// <summary>RN-505: dias possíveis para o vencimento da primeira parcela.</summary>
    public IReadOnlyList<int> ReadPossibleGracePeriodsInDays()
        => Deserialize<int>(PossibleGracePeriodsInDaysJson);

    /// <summary>RN-510: documentos exigidos pela Seguradora, informativos ao corretor.</summary>
    public IReadOnlyList<QuotationRequiredDocument> ReadRequiredDocuments()
        => Deserialize<QuotationRequiredDocument>(RequiredDocumentsJson);

    /// <summary>RN-505: o parcelamento escolhido tem de constar entre os informados pela Seguradora.</summary>
    public bool HasInstallmentOption(int installmentNumber)
        => ReadInstallmentOptions().Any(option => option.Number == installmentNumber);

    /// <summary>RN-505: o vencimento escolhido tem de constar entre os informados pela Seguradora.</summary>
    public bool HasGracePeriodOption(int gracePeriodInDays)
        => ReadPossibleGracePeriodsInDays().Contains(gracePeriodInDays);

    private static string? SerializeOrNull<T>(List<T>? items)
        => items is null || items.Count == 0 ? null : JsonSerializer.Serialize(items);

    private static IReadOnlyList<T> Deserialize<T>(string? json)
        => string.IsNullOrWhiteSpace(json)
            ? []
            : JsonSerializer.Deserialize<List<T>>(json) ?? [];

    /// <summary>RN-079: captura a minuta (Tags/Cláusulas preenchidas) da Cotação selecionada.</summary>
    public void SetMinuta(string? tagsJson, string? clausesJson)
    {
        MinutaTagsJson = tagsJson;
        MinutaClausesJson = clausesJson;
    }

    /// <summary>
    /// RN-059: seguíveis são Automatic e Analysis+Underwriting; as demais não. CCG não bloqueia
    /// (ADR-064) — a exigência só é enforçada na emissão.
    /// </summary>
    public bool IsFollowable
        => Result == EQuotationResult.ReadyForEmission
           || (Result == EQuotationResult.Analysis && AnalysisTrack == EAnalysisTrack.Underwriting);
}

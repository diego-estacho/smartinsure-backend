using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Enumerators;

namespace SmartInsure.Integration.CalculationEngines.PlugV2;

/// <summary>
/// Camada anticorrupção (ADR-045, ADR-064): traduz o status imediato do PlugV2 (os 11 valores do eixo
/// imediato) para o resultado estável do domínio (EQuotationResult + esteira + motivos), num ÚNICO
/// ponto. Todo status NÃO reconhecido recai em Unrecognized (sem prêmio, não seguível) — jamais
/// convertido em silêncio para outra classificação. Prêmio só é lido em Automatic. CCG é ortogonal,
/// capturado independentemente da classificação. Nada do modelo do fornecedor sai daqui (ADR-028).
/// </summary>
public static class PlugV2QuotationAclMapper
{
    public static QuotationResult Map(
        PlugV2CotationData response, bool hasError = false, IReadOnlyList<string>? envelopeErrors = null)
    {
        var ccg = response.PolicyHolderCcg;

        // Motivos do payload (Erros) mais os erros do envelope — preserva a mensagem específica do gateway.
        var providerReasons = (response.Erros ?? [])
            .Concat(envelopeErrors ?? [])
            .Where(reason => !string.IsNullOrWhiteSpace(reason))
            .Distinct()
            .ToList();

        // Gateway sinalizou erro (HasError): jamais seguível. Não confia no status nem no prêmio de um payload
        // marcado como erro — classifica Indisponível com os motivos do provedor (e captura o CCG). ADR-064.
        if (hasError)
        {
            return Unavailable(providerReasons, "Falha sinalizada pela Seguradora.", ccg);
        }

        var status = (EPlugApiStatus)(response.ResponseStatus?.Status ?? (int)EPlugApiStatus.Unknow);

        return status switch
        {
            EPlugApiStatus.Success => Automatic(response, ccg),
            EPlugApiStatus.KanbanSubscricao => Analysis(EAnalysisTrack.Underwriting, ccg),
            EPlugApiStatus.KanbanCadastro => Analysis(EAnalysisTrack.Registration, ccg),
            EPlugApiStatus.KanbanPep => Analysis(EAnalysisTrack.Pep, ccg),
            EPlugApiStatus.KanbanCredito => Analysis(EAnalysisTrack.Credit, ccg),
            EPlugApiStatus.KanbanResseguro => Analysis(EAnalysisTrack.Reinsurance, ccg),
            EPlugApiStatus.ModalidadeIndisponivel => Unavailable(providerReasons, "Modalidade indisponível.", ccg),
            EPlugApiStatus.CoberturaIndisponivel => Unavailable(providerReasons, "Cobertura indisponível.", ccg),
            EPlugApiStatus.TomadorNomeado => Unavailable(providerReasons, "Tomador nomeado.", ccg),
            EPlugApiStatus.Error => Unavailable(providerReasons, "Falha técnica na Seguradora.", ccg),

            // Unknow e QUALQUER status fora do conjunto conhecido → Unrecognized (ADR-064): nunca silêncio.
            _ => Unrecognized(),
        };
    }

    private static QuotationResult Automatic(PlugV2CotationData response, PlugV2CcgResult? ccg)
        => new()
        {
            Result = EQuotationResult.Automatic,
            Premium = response.InsurancePremium,
            CommissionPercentage = response.ComissionPercentage,
            CommissionValue = response.ComissionValue,
            Tax = response.Tax,
            AvailableLimit = response.PolicyHolderAvailableLimit,
            ProposalExternalId = NullIfBlank(response.ProposalUniqueId),
            ProposalNumber = NullIfBlank(response.ProposalNumber),
            RequiresCcg = ccg?.RequiresCcg ?? false,
            CcgMaxLimitWithoutNeed = ccg?.MaxLimitWithoutNeedCcg,
            CcgSigned = ccg?.HasSignedCcg ?? false,
        };

    private static QuotationResult Analysis(EAnalysisTrack track, PlugV2CcgResult? ccg)
        => new()
        {
            Result = EQuotationResult.Analysis,
            AnalysisTrack = track,
            RequiresCcg = ccg?.RequiresCcg ?? false,
            CcgMaxLimitWithoutNeed = ccg?.MaxLimitWithoutNeedCcg,
            CcgSigned = ccg?.HasSignedCcg ?? false,
        };

    private static QuotationResult Unavailable(List<string> providerReasons, string fallbackReason, PlugV2CcgResult? ccg)
        => new()
        {
            Result = EQuotationResult.Unavailable,
            Reasons = providerReasons.Count > 0 ? providerReasons : [fallbackReason],
            RequiresCcg = ccg?.RequiresCcg ?? false,
            CcgMaxLimitWithoutNeed = ccg?.MaxLimitWithoutNeedCcg,
            CcgSigned = ccg?.HasSignedCcg ?? false,
        };

    private static QuotationResult Unrecognized()
        => new() { Result = EQuotationResult.Unrecognized };

    private static string? NullIfBlank(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value;
}

using System.Text.Json;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Enumerators;

namespace SmartInsure.Integration.CalculationEngines.PlugV2;

/// <summary>
/// Camada anticorrupção (ADR-045) do resultado de cotação: traduz o status do PlugV2 para a
/// classificação de domínio + esteira/motivos + CCG (ADR-064). É o ÚNICO lugar do de-para.
/// Conjunto do eixo imediato conferido na fonte (gateway): 11 valores; qualquer outro → Unrecognized,
/// nunca convertido em silêncio para Automatic. Não lança — falha de transporte é do client.
/// </summary>
public static class PlugV2QuotationAclMapper
{
    private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };

    public static QuotationEngineResult Map(string rawJson)
    {
        PlugV2RunQuotationResponse? envelope;

        try
        {
            envelope = JsonSerializer.Deserialize<PlugV2RunQuotationResponse>(rawJson, Options);
        }
        catch (JsonException)
        {
            return Unrecognized();
        }

        // Sem corpo classificável → Não-reconhecido (visível, sem prêmio, não seguível).
        if (envelope?.Response is not { } data)
        {
            return Unrecognized();
        }

        var (result, track) = MapStatus(data.Status);
        var reasons = CollectReasons(result, data, track);

        return result switch
        {
            EQuotationResult.Automatic => new QuotationEngineResult
            {
                Result = EQuotationResult.Automatic,
                Premium = data.InsurancePremium,
                CommissionPercentage = data.ComissionPercentage,
                CommissionValue = data.ComissionValue,
                Tax = data.Tax,
                AvailableLimit = data.PolicyHolderAvailableLimit,
                ProposalExternalId = NullIfBlank(data.ProposalUniqueId),
                ProposalNumber = NullIfBlank(data.ProposalNumber),
                RequiresCcg = data.Ccg?.RequiresCcg ?? false,
                CcgMaxLimitWithoutNeed = data.Ccg?.MaxLimitWithoutNeed,
                CcgSigned = data.Ccg?.HasSigned ?? false,
                Reasons = reasons,
            },
            _ => new QuotationEngineResult
            {
                Result = result,
                AnalysisTrack = track,
                AvailableLimit = data.PolicyHolderAvailableLimit,
                ProposalExternalId = NullIfBlank(data.ProposalUniqueId),
                ProposalNumber = NullIfBlank(data.ProposalNumber),
                RequiresCcg = data.Ccg?.RequiresCcg ?? false,
                CcgMaxLimitWithoutNeed = data.Ccg?.MaxLimitWithoutNeed,
                CcgSigned = data.Ccg?.HasSigned ?? false,
                Reasons = reasons,
            },
        };
    }

    /// <summary>De-para do eixo imediato (ADR-064). Desconhecido/UNKNOW → Unrecognized.</summary>
    private static (EQuotationResult Result, EAnalysisTrack? Track) MapStatus(string? status)
        => status?.Trim().ToUpperInvariant() switch
        {
            "SUCCESS" => (EQuotationResult.Automatic, null),
            "KANBAN_SUBSCRICAO" => (EQuotationResult.Analysis, EAnalysisTrack.Underwriting),
            "KANBAN_CADASTRO" => (EQuotationResult.Analysis, EAnalysisTrack.Registration),
            "KANBAN_PEP" => (EQuotationResult.Analysis, EAnalysisTrack.Pep),
            "KANBAN_CREDITO" => (EQuotationResult.Analysis, EAnalysisTrack.Credit),
            "KANBAN_RESSEGURO" => (EQuotationResult.Analysis, EAnalysisTrack.Reinsurance),
            "MODALIDADE_INDISPONIVEL" => (EQuotationResult.Unavailable, null),
            "COBERTURA_INDISPONIVEL" => (EQuotationResult.Unavailable, null),
            "TOMADOR_NOMEADO" => (EQuotationResult.Unavailable, null),
            "ERROR" => (EQuotationResult.Unavailable, null),
            _ => (EQuotationResult.Unrecognized, null),
        };

    private static IReadOnlyList<string> CollectReasons(
        EQuotationResult result, PlugV2QuotationData data, EAnalysisTrack? track)
    {
        var reasons = (data.Erros ?? [])
            .Where(reason => !string.IsNullOrWhiteSpace(reason))
            .ToList();

        // Indisponibilidade sem motivo explícito recebe o motivo derivado do status (RN-058).
        if (reasons.Count == 0 && result == EQuotationResult.Unavailable)
        {
            var derived = DerivedReason(data.Status);
            if (derived is not null)
            {
                reasons.Add(derived);
            }
        }

        return reasons;
    }

    private static string? DerivedReason(string? status)
        => status?.Trim().ToUpperInvariant() switch
        {
            "MODALIDADE_INDISPONIVEL" => "Modalidade indisponível para a Seguradora.",
            "COBERTURA_INDISPONIVEL" => "Cobertura indisponível para a Seguradora.",
            "TOMADOR_NOMEADO" => "Tomador nomeado a outra Corretora.",
            "ERROR" => "Falha na integração com a Seguradora.",
            _ => null,
        };

    private static QuotationEngineResult Unrecognized()
        => new() { Result = EQuotationResult.Unrecognized };

    private static string? NullIfBlank(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value;
}

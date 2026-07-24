using System.Text.Json;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Enumerators;
using SmartInsure.Integration.CalculationEngines.PlugV2.Models;

namespace SmartInsure.Integration.CalculationEngines.PlugV2;

/// <summary>
/// Camada anticorrupção (ADR-045): traduz o payload do PlugV2 (GetAdditionalCoverages) para o
/// contrato do motor (`AdditionalCoverageResult`). Nada do modelo do fornecedor sai daqui.
/// Envelope com erro, corpo nulo ou status diferente de 200 vira falha daquela modalidade
/// (IsSuccess=false, RN-046). Ramo pelo BranchCode SUSEP: 0775 = Público, 0776 = Privado (75/76 nos
/// formatos curtos observados em modalidades); cobertura de ramo não mapeável é descartada (RN-042).
/// O nome não é normalizado aqui — a normalização e o descarte de sem-nome são regra de negócio do
/// importador (RN-040).
/// </summary>
public static class PlugV2AdditionalCoveragesAclMapper
{
    private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };

    public static ImportedAdditionalCoverageResult Map(string rawJson)
    {
        PlugV2BaseResponse<PlugV2AdditionalCoveragesResponse>? envelope;

        try
        {
            envelope = JsonSerializer.Deserialize<PlugV2BaseResponse<PlugV2AdditionalCoveragesResponse>>(
                rawJson, Options);
        }
        catch (JsonException)
        {
            envelope = null;
        }

        if (envelope is null || envelope.HasError || envelope.StatusCode != 200 || envelope.Response is null)
        {
            var message = envelope?.Errors is { Count: > 0 }
                ? string.Join("; ", envelope.Errors)
                : "Resposta inválida do PlugV2 em GetAdditionalCoverages.";
            return new ImportedAdditionalCoverageResult(false, [], message);
        }

        var coverages = new List<ImportedAdditionalCoverageData>();

        foreach (var entry in envelope.Response.AdditionalCoverages ?? [])
        {
            var item = entry.AdditionalCoverages;

            if (item is null)
            {
                continue;
            }

            if (!TryMapBranch(entry.BranchCode, out var branch))
            {
                // RN-042: ramo não mapeável — não há como confirmar o vínculo; descartada.
                continue;
            }

            coverages.Add(new ImportedAdditionalCoverageData(
                Name: item.Name ?? string.Empty,
                SourceUniqueId: item.UniqueId,
                InsuredAmountCalculationType: item.InsuredAmountCalculationType,
                AllowManualEdit: item.AllowManualEdit,
                Branch: branch));
        }

        return new ImportedAdditionalCoverageResult(true, coverages, null);
    }

    private static bool TryMapBranch(string? branchCode, out ESuretyBranch branch)
    {
        switch (branchCode?.Trim())
        {
            case "75":
            case "775":
            case "0775":
                branch = ESuretyBranch.Public;
                return true;
            case "76":
            case "776":
            case "0776":
                branch = ESuretyBranch.Private;
                return true;
            default:
                branch = default;
                return false;
        }
    }
}

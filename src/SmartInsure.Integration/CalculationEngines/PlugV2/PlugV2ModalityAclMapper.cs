using System.Text.Json;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Enumerators;
using SmartInsure.Integration.CalculationEngines.PlugV2.Models;

namespace SmartInsure.Integration.CalculationEngines.PlugV2;

/// <summary>
/// Camada anticorrupção (ADR-045): traduz o payload do PlugV2 (GetGroupAndModalities) para o
/// contrato do motor (`ImportedCatalogResult`). Nada do modelo do fornecedor sai daqui.
/// Ramo pelo BranchCode: 75 = Público, 76 = Privado (dev observado 2026-07-22). Modalidade de
/// ramo desconhecido é descartada (não há como posicioná-la com segurança — RN-035).
/// </summary>
public static class PlugV2ModalityAclMapper
{
    private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };

    /// <summary>Retorna (resultado, houveErroNoEnvelope, mensagemDeErro).</summary>
    public static (ImportedCatalogResult Result, bool EnvelopeError, string? ErrorMessage) Map(string rawJson)
    {
        var envelope = JsonSerializer.Deserialize<PlugV2BaseResponse<List<PlugV2GroupsAndModalities>>>(rawJson, Options);

        if (envelope is null || envelope.HasError || envelope.StatusCode != 200 || envelope.Response is null)
        {
            var message = envelope?.Errors is { Count: > 0 }
                ? string.Join("; ", envelope.Errors)
                : "Resposta inválida do PlugV2 em GetGroupAndModalities.";
            return (new ImportedCatalogResult([]), true, message);
        }

        var insurers = new List<ImportedInsurerCatalog>(envelope.Response.Count);

        foreach (var entry in envelope.Response)
        {
            var referenceExternalId = entry.Insurance?.InsuranceUniqueId ?? string.Empty;
            var insuranceName = entry.Insurance?.Name ?? string.Empty;
            var modalities = new List<ImportedModalityData>();

            foreach (var global in entry.GlobalModalities ?? [])
            {
                foreach (var element in global.Modalities ?? [])
                {
                    if (!TryMapModality(element, global, out var data))
                    {
                        continue;
                    }

                    modalities.Add(data);
                }
            }

            insurers.Add(new ImportedInsurerCatalog(
                referenceExternalId, insuranceName, entry.IsSuccess, modalities));
        }

        return (new ImportedCatalogResult(insurers), false, null);
    }

    private static bool TryMapModality(
        JsonElement element, PlugV2GlobalModality global, out ImportedModalityData data)
    {
        data = null!;

        var sourceId = ReadString(element, "ModalityUniqueId");
        var groupSourceId = ReadString(element, "ModalityGroupUniqueId");

        if (string.IsNullOrWhiteSpace(sourceId) || string.IsNullOrWhiteSpace(groupSourceId))
        {
            return false;
        }

        if (!TryMapBranch(ReadString(element, "BranchCode"), out var branch))
        {
            return false;
        }

        data = new ImportedModalityData(
            SourceId: sourceId,
            OriginName: ReadString(element, "Name") ?? sourceId,
            Branch: branch,
            EngineModalityId: global.Id.ToString(),
            EngineModalityName: global.Name,
            GroupSourceId: groupSourceId,
            GroupName: ReadString(element, "ModalityGroupName") ?? groupSourceId,
            GroupType: ReadString(element, "ModalityGroupType"),
            RawParameters: element.GetRawText());

        return true;
    }

    private static bool TryMapBranch(string? branchCode, out ESuretyBranch branch)
    {
        switch (branchCode?.Trim())
        {
            case "75":
                branch = ESuretyBranch.Public;
                return true;
            case "76":
                branch = ESuretyBranch.Private;
                return true;
            default:
                branch = default;
                return false;
        }
    }

    private static string? ReadString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var value))
        {
            return null;
        }

        return value.ValueKind switch
        {
            JsonValueKind.String => value.GetString(),
            JsonValueKind.Number => value.ToString(),
            _ => null,
        };
    }
}

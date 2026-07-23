using System.Text.Json;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Integration.CalculationEngines.PlugV2.Models;

namespace SmartInsure.Integration.CalculationEngines.PlugV2;

/// <summary>
/// Camada anticorrupção (ADR-045): traduz o payload do PlugV2 (GetModalityObject) para o
/// contrato do motor (`ModalityObjectResult`). Nada do modelo do fornecedor sai daqui.
/// RN-042: envelope com erro ou resposta nula é falha isolada da modalidade — nunca lança.
/// </summary>
public static class PlugV2ModalityObjectAclMapper
{
    private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };

    public static ModalityObjectResult Map(string rawJson)
    {
        PlugV2BaseResponse<PlugV2ModalityObjectResponse>? envelope;

        try
        {
            envelope = JsonSerializer.Deserialize<PlugV2BaseResponse<PlugV2ModalityObjectResponse>>(rawJson, Options);
        }
        catch (JsonException)
        {
            return new ModalityObjectResult(true, null, null, []);
        }

        if (envelope is null || envelope.HasError || envelope.Response is null)
        {
            return new ModalityObjectResult(true, null, null, []);
        }

        var jsonTag = NullIfBlank(envelope.Response.Object?.JsonTag);
        var objectText = NullIfBlank(envelope.Response.Object?.Text);

        var clauses = new List<ModalityClauseData>();

        foreach (var clause in envelope.Response.ParticularClauses ?? [])
        {
            // RN-041 (caso limite): cláusula sem id é descartada — não vira item órfão.
            // Id == 0 é um id externo válido (não é "ausente") e deve ser mantido.
            if (clause.Id is not { } id)
            {
                continue;
            }

            clauses.Add(new ModalityClauseData(
                ExternalId: id.ToString(System.Globalization.CultureInfo.InvariantCulture),
                Name: clause.Name ?? string.Empty,
                Text: NullIfBlank(clause.Text),
                JsonTag: NullIfBlank(clause.JsonTag)));
        }

        return new ModalityObjectResult(false, jsonTag, objectText, clauses);
    }

    private static string? NullIfBlank(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value;
}

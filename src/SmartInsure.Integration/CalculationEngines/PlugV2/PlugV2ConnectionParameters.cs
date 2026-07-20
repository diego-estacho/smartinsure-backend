using System.Text.Json;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Integration.CalculationEngines.PlugV2;

/// <summary>
/// Parâmetros de conexão do vínculo exigidos pelo motor PlugV2 (RN-022):
/// endereço do gateway e chave de acesso do par Corretora×Seguradora.
/// Os demais dados da chamada não vivem aqui: o CNPJ da Corretora vem da Habilitação
/// (BrokerageId → Pessoa) e o identificador externo da Seguradora vem do catálogo
/// (Insurer.ReferenceExternalId).
/// </summary>
public sealed record PlugV2ConnectionParameters
{
    public required string BaseUrl { get; init; }

    public required string Key { get; init; }

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static PlugV2ConnectionParameters Parse(string? connectionParameters)
    {
        if (string.IsNullOrWhiteSpace(connectionParameters))
        {
            throw new BusinessRuleException(
                "A habilitação não possui os parâmetros de conexão exigidos pelo motor PlugV2 (baseUrl e key).");
        }

        PlugV2ConnectionParameters? parsed;

        try
        {
            parsed = JsonSerializer.Deserialize<PlugV2ConnectionParameters>(connectionParameters, JsonOptions);
        }
        catch (JsonException)
        {
            throw new BusinessRuleException(
                "Os parâmetros de conexão da habilitação são inválidos para o motor PlugV2 (baseUrl e key).");
        }

        if (parsed is null
            || string.IsNullOrWhiteSpace(parsed.BaseUrl)
            || string.IsNullOrWhiteSpace(parsed.Key)
            || !Uri.TryCreate(parsed.BaseUrl, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new BusinessRuleException(
                "Os parâmetros de conexão da habilitação são inválidos para o motor PlugV2 (baseUrl e key).");
        }

        return parsed;
    }
}

using System.Net.Http.Json;
using System.Text.Json;
using SmartInsure.Core.Abstractions.Services;

namespace SmartInsure.Integration.CalculationEngines.PlugV2;

/// <summary>
/// Cliente HTTP do PlugV2 para GetGroupAndModalities (RN-034). A base URL é por Habilitação
/// (ConnectionParameters), então a URL é montada por chamada; a resiliência (ADR-044) vem do
/// HttpClient nomeado. Falha de transporte ou envelope de erro sobe como exceção — o importer
/// isola a falha por Corretora (RN-038).
/// </summary>
public sealed class PlugV2ModalityImportClient(IHttpClientFactory httpClientFactory)
{
    public const string HttpClientName = "PlugV2Modalities";

    private const string OperationPath = "/GetGroupAndModalities";

    private static readonly JsonSerializerOptions BodyOptions = new();

    public async Task<ImportedCatalogResult> GetGroupAndModalitiesAsync(
        PlugV2ConnectionParameters connection, string brokerCnpj, CancellationToken cancellationToken)
    {
        var client = httpClientFactory.CreateClient(HttpClientName);
        var url = $"{connection.BaseUrl.TrimEnd('/')}{OperationPath}";

        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.TryAddWithoutValidation("application-key-v2", connection.Key);
        request.Content = JsonContent.Create(new { BrokerCnpj = brokerCnpj }, options: BodyOptions);

        using var response = await client.SendAsync(request, cancellationToken);
        var raw = await response.Content.ReadAsStringAsync(cancellationToken);

        var (result, envelopeError, errorMessage) = PlugV2ModalityAclMapper.Map(raw);

        if (envelopeError)
        {
            throw new InvalidOperationException(
                $"PlugV2 GetGroupAndModalities recusou a corretora {brokerCnpj}: {errorMessage}");
        }

        return result;
    }
}

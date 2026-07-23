using System.Net.Http.Json;
using System.Text.Json;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Integration.CalculationEngines.PlugV2;

/// <summary>
/// Cliente HTTP do PlugV2 para GetModalityObject (RN-040). A base URL é por Habilitação
/// (ConnectionParameters), então a URL é montada por chamada; a resiliência (ADR-044) vem do
/// HttpClient nomeado. Falha de transporte sobe como exceção — o importer isola a falha por
/// modalidade (RN-042); a tradução do envelope de negócio fica na ACL, que não lança.
/// </summary>
public sealed class PlugV2ModalityObjectClient(IHttpClientFactory httpClientFactory)
{
    public const string HttpClientName = "PlugV2ModalityObject";

    private const string OperationPath = "/GetModalityObject";

    private static readonly JsonSerializerOptions BodyOptions = new();

    public async Task<ModalityObjectResult> GetModalityObjectAsync(
        PlugV2ConnectionParameters connection, string brokerCnpj, string modalityUniqueId, CancellationToken cancellationToken)
    {
        var client = httpClientFactory.CreateClient(HttpClientName);
        var url = $"{connection.BaseUrl.TrimEnd('/')}{OperationPath}";

        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.TryAddWithoutValidation("application-key-v2", connection.Key);
        request.Content = JsonContent.Create(
            new { BrokerCnpj = brokerCnpj, ModalityUniqueId = modalityUniqueId }, options: BodyOptions);

        using var response = await client.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new CalculationEngineException(
                $"PlugV2 GetModalityObject retornou status {response.StatusCode} para a modalidade {modalityUniqueId}.");
        }

        var raw = await response.Content.ReadAsStringAsync(cancellationToken);

        return PlugV2ModalityObjectAclMapper.Map(raw);
    }
}

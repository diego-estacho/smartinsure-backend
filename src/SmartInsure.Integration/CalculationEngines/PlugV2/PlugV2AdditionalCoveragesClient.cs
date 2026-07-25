using System.Net.Http.Json;
using System.Text.Json;
using SmartInsure.Core.Abstractions.Services;

namespace SmartInsure.Integration.CalculationEngines.PlugV2;

/// <summary>
/// Cliente HTTP do PlugV2 para GetAdditionalCoverages (RN-042/RN-044): uma chamada por Modalidade
/// Importada. A base URL é por Habilitação (ConnectionParameters); a resiliência (ADR-044) vem do
/// HttpClient nomeado. Falha de transporte sobe como exceção — o importador isola a falha por
/// modalidade (RN-046); erro de envelope volta como resultado IsSuccess=false (tratado na ACL).
/// </summary>
public sealed class PlugV2AdditionalCoveragesClient(IHttpClientFactory httpClientFactory)
{
    public const string HttpClientName = "PlugV2AdditionalCoverages";

    private const string OperationPath = "/GetAdditionalCoverages";

    private static readonly JsonSerializerOptions BodyOptions = new();

    public async Task<ImportedAdditionalCoverageResult> GetAdditionalCoveragesAsync(
        PlugV2ConnectionParameters connection,
        string brokerCnpj,
        string insuranceUniqueId,
        string modalityName,
        string? modalityGroupType,
        CancellationToken cancellationToken)
    {
        var client = httpClientFactory.CreateClient(HttpClientName);
        var url = $"{connection.BaseUrl.TrimEnd('/')}{OperationPath}";

        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.TryAddWithoutValidation("application-key-v2", connection.Key);
        request.Content = JsonContent.Create(
            new
            {
                BrokerCnpj = brokerCnpj,
                InsuranceUniqueId = insuranceUniqueId,
                ModalityName = modalityName,
                ModalityGroupType = modalityGroupType,
            },
            options: BodyOptions);

        using var response = await client.SendAsync(request, cancellationToken);
        var raw = await response.Content.ReadAsStringAsync(cancellationToken);

        return PlugV2AdditionalCoveragesAclMapper.Map(raw);
    }
}

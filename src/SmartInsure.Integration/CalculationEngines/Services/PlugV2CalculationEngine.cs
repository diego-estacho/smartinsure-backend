using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;
using SmartInsure.Integration.CalculationEngines.PlugV2;

namespace SmartInsure.Integration.CalculationEngines.Services;

/// <summary>
/// Motor de Cálculo PlugV2 (RN-023): único motor disponível nesta fase. A importação de
/// modalidades (RN-034) consome o gateway com os parâmetros de conexão da Habilitação resolvida
/// (baseUrl/key) e o CNPJ da Corretora do vínculo; a tradução do payload fica na ACL (ADR-045).
/// O client de importação é resolvido sob demanda — o núcleo do motor (Engine, validação de
/// parâmetros) não depende da infraestrutura HTTP. As demais operações entram por jornada (OPEN-07).
/// </summary>
public sealed class PlugV2CalculationEngine(
    IServiceProvider serviceProvider,
    IHttpClientFactory httpClientFactory) : ICalculationEngine
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private const string ClientName = "PlugV2";
    private const int TimeoutSeconds = 30;

    public ECalculationEngine Engine => ECalculationEngine.PlugV2;

    public void EnsureValidConnectionParameters(string? connectionParameters)
        => PlugV2ConnectionParameters.Parse(connectionParameters);

    public Task<ImportedCatalogResult> GetGroupAndModalitiesAsync(
        string? connectionParameters, string brokerCnpj, CancellationToken cancellationToken)
    {
        var connection = PlugV2ConnectionParameters.Parse(connectionParameters);
        var importClient = serviceProvider.GetRequiredService<PlugV2ModalityImportClient>();
        return importClient.GetGroupAndModalitiesAsync(connection, brokerCnpj, cancellationToken);
    }

    public Task<ImportedAdditionalCoverageResult> GetAdditionalCoveragesAsync(
        string? connectionParameters,
        string brokerCnpj,
        string insuranceUniqueId,
        string modalityName,
        string? modalityGroupType,
        CancellationToken cancellationToken)
    {
        var connection = PlugV2ConnectionParameters.Parse(connectionParameters);
        var coveragesClient = serviceProvider.GetRequiredService<PlugV2AdditionalCoveragesClient>();
        return coveragesClient.GetAdditionalCoveragesAsync(
            connection, brokerCnpj, insuranceUniqueId, modalityName, modalityGroupType, cancellationToken);
    }

    /// <summary>RN-029: consulta limites de crédito do tomador junto à Seguradora via PlugV2.</summary>
    public async Task<PolicyHolderLimitsAndRates?> GetPolicyHolderLimitsAndRatesAsync(
        string? connectionParameters,
        string brokerageCnpj,
        string policyHolderCnpj,
        string insurerExternalId,
        CancellationToken cancellationToken)
    {
        var config = PlugV2ConnectionParameters.Parse(connectionParameters);

        var client = httpClientFactory.CreateClient(ClientName);
        // Barra final preserva o caminho do gateway (ex.: /qa/garantia/plugv2) na resolução da URI relativa.
        client.BaseAddress = new Uri(config.BaseUrl.EndsWith('/') ? config.BaseUrl : config.BaseUrl + "/");
        client.Timeout = TimeSpan.FromSeconds(TimeoutSeconds);

        var request = new PlugV2GetPolicyHolderLimitsAndRatesRequest
        {
            BrokerCnpj = brokerageCnpj,
            PolicyHolderCnpj = policyHolderCnpj,
            InsuranceUniqueId = insurerExternalId,
        };

        try
        {
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "GetPolicyHolderLimitsAndRates")
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(request, JsonOptions),
                    System.Text.Encoding.UTF8,
                    "application/json"),
            };

            httpRequest.Headers.Add("application-key-v2", config.Key);

            using var httpResponse = await client.SendAsync(httpRequest, cancellationToken);

            if (!httpResponse.IsSuccessStatusCode)
            {
                throw new CalculationEngineException(
                    $"PlugV2 retornou status {httpResponse.StatusCode} na consulta de limites de crédito.");
            }

            var responseContent = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

            var response = JsonSerializer.Deserialize<PlugV2GetPolicyHolderLimitsAndRatesResponse>(
                responseContent, JsonOptions);

            // RN-030: resposta nula ou com erro => indisponível.
            if (response is null || response.HasError || response.Response?.Count == 0)
            {
                return null;
            }

            // Localizar resposta da Seguradora pelo InsuranceUniqueId (case-insensitive).
            var insurerResponse = response.Response!.FirstOrDefault(r =>
                r.Insurance?.InsuranceUniqueId?.Equals(insurerExternalId, StringComparison.OrdinalIgnoreCase) == true);

            // RN-030: Seguradora não encontrada na resposta => indisponível.
            if (insurerResponse is null || insurerResponse.LimitsAndRates?.Count == 0)
            {
                return null;
            }

            // RN-029: agregar LimitsAndRates por ModalityGroupName, selecionando a linha com maior AvailableLimit.
            var groups = insurerResponse.LimitsAndRates
                .GroupBy(l => l.ModalityGroupName)
                .Select(g => g.OrderByDescending(l => l.AvailableLimit).First())
                .Select(l => new PolicyHolderLimitGroup
                {
                    GroupName = l.ModalityGroupName,
                    GroupType = l.ModalityGroupType,
                    AvailableLimit = l.AvailableLimit,
                    RevisedLimit = l.LimitRevised,
                    Rate = l.Tax,
                })
                .ToList();

            return new PolicyHolderLimitsAndRates
            {
                PolicyHolderName = insurerResponse.PolicyHolderName,
                Groups = groups.AsReadOnly(),
            };
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (CalculationEngineException)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new CalculationEngineException(
                "Falha ao consultar limites de crédito no motor PlugV2.",
                exception);
        }
    }
}

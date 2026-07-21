using System.Text.Json;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;
using SmartInsure.Integration.CalculationEngines.PlugV2;

namespace SmartInsure.Integration.CalculationEngines.Services;

/// <summary>
/// Motor de Cálculo PlugV2 (RN-023): único motor disponível nesta fase. As operações do
/// contrato (cotar, prêmio, dados de apoio, emissão, cancelamento) entram nas demandas
/// de cada jornada (OPEN-07), consumindo o gateway com os parâmetros de conexão da
/// Habilitação resolvida (baseUrl/key), o CNPJ da Corretora do vínculo e o
/// ReferenceExternalId da Seguradora.
/// </summary>
public sealed class PlugV2CalculationEngine(IHttpClientFactory httpClientFactory) : ICalculationEngine
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private const string ClientName = "PlugV2";
    private const int TimeoutSeconds = 30;

    public ECalculationEngine Engine => ECalculationEngine.PlugV2;

    public void EnsureValidConnectionParameters(string? connectionParameters)
        => PlugV2ConnectionParameters.Parse(connectionParameters);

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
        client.BaseAddress = new Uri(config.BaseUrl);
        client.Timeout = TimeSpan.FromSeconds(TimeoutSeconds);

        var request = new PlugV2GetPolicyHolderLimitsAndRatesRequest
        {
            BrokerCnpj = brokerageCnpj,
            PolicyHolderCnpj = policyHolderCnpj,
            InsuranceUniqueId = insurerExternalId,
        };

        try
        {
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/GetPolicyHolderLimitsAndRates")
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

            if (response is null || !response.Success)
            {
                return null;
            }

            return new PolicyHolderLimitsAndRates
            {
                TraditionalLimit = response.TraditionalLimit,
                TraditionalRate = response.TraditionalRate,
                JudicialLimit = response.JudicialLimit,
                JudicialRate = response.JudicialRate,
                JudicialFiscalRate = response.JudicialFiscalRate,
                FinancialLimit = response.FinancialLimit,
                FinancialRate = response.FinancialRate,
                LimitValidUntil = response.LimitValidUntil,
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

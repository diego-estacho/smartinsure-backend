using System.Net.Http.Json;
using System.Text.Json;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Integration.CalculationEngines.PlugV2;

/// <summary>
/// Cliente HTTP do PlugV2 para POST /Cotation (RN-056/057). Base URL por Habilitação
/// (ConnectionParameters), montada por chamada; resiliência (ADR-044) do HttpClient nomeado.
/// Falha de transporte (não-2xx / exceção) sobe como CalculationEngineException — a aplicação
/// isola a falha por Seguradora (RN-057). A tradução do resultado de negócio fica na ACL, que não lança.
/// </summary>
public sealed class PlugV2RunQuotationClient(IHttpClientFactory httpClientFactory)
{
    public const string HttpClientName = "PlugV2RunQuotation";

    private const string OperationPath = "/Cotation";

    private static readonly JsonSerializerOptions BodyOptions = new(JsonSerializerDefaults.Web);

    public async Task<QuotationEngineResult> RunQuotationAsync(
        PlugV2ConnectionParameters connection,
        QuotationEngineRequest request,
        CancellationToken cancellationToken)
    {
        var client = httpClientFactory.CreateClient(HttpClientName);
        var url = $"{connection.BaseUrl.TrimEnd('/')}{OperationPath}";

        var body = new PlugV2RunQuotationRequest
        {
            BrokerCnpj = request.BrokerCnpj,
            PolicyHolderCnpj = request.PolicyHolderCnpj,
            InsuredCpfCnpj = request.InsuredCpfCnpj,
            InsuranceUniqueId = request.InsuranceUniqueId,
            ModalityGlobalId = request.ModalityGlobalId,
            ModalityName = request.ModalityName,
            ModalityGroupType = request.ModalityGroupType,
            InsuredAmountValue = request.InsuredAmount,
            StartDate = request.CoverageStartDate,
            EndDate = request.CoverageEndDate,
            AdditionalCoverages = BuildCoverages(request),
        };

        try
        {
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = JsonContent.Create(body, options: BodyOptions),
            };
            httpRequest.Headers.TryAddWithoutValidation("application-key-v2", connection.Key);

            using var response = await client.SendAsync(httpRequest, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new CalculationEngineException(
                    $"PlugV2 /Cotation retornou status {response.StatusCode} para a Seguradora {request.InsuranceUniqueId}.");
            }

            var raw = await response.Content.ReadAsStringAsync(cancellationToken);

            return PlugV2QuotationAclMapper.Map(raw);
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
                $"Falha ao cotar a Seguradora {request.InsuranceUniqueId} no motor PlugV2.", exception);
        }
    }

    // Coberturas Adicionais provisórias (2 booleanos, RN-051) → identificadores para o gateway.
    private static IReadOnlyList<string> BuildCoverages(QuotationEngineRequest request)
    {
        var coverages = new List<string>();

        if (request.IncludesPenaltyCoverage)
        {
            coverages.Add("MULTA");
        }

        if (request.IncludesLaborCoverage)
        {
            coverages.Add("TRABALHISTA_PREVIDENCIARIA");
        }

        return coverages;
    }
}

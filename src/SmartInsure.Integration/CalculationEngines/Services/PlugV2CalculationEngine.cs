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

    public Task<ModalityObjectResult> GetModalityObjectAsync(
        string? connectionParameters, string brokerCnpj, string modalityUniqueId, CancellationToken cancellationToken)
    {
        var connection = PlugV2ConnectionParameters.Parse(connectionParameters);
        var client = serviceProvider.GetRequiredService<PlugV2ModalityObjectClient>();
        return client.GetModalityObjectAsync(connection, brokerCnpj, modalityUniqueId, cancellationToken);
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

    /// <summary>
    /// RN-057/RN-058: solicita uma Cotação à Seguradora (POST /Cotation) e traduz o resultado pela ACL
    /// (ADR-064, <see cref="PlugV2QuotationAclMapper"/>). O gateway envelopa a resposta (BaseResponse,
    /// confirmado no probe dev 2026-07-28): a Cotação vem em Response; HasError/erro HTTP sinaliza falha.
    /// Falha de transporte/desserialização sobe como CalculationEngineException — o consumidor registra a
    /// Cotação como falha (RN-057, sem retry).
    /// </summary>
    public async Task<QuotationResult> RunQuotationAsync(
        string? connectionParameters, QuotationRequestInput request, CancellationToken cancellationToken)
    {
        var config = PlugV2ConnectionParameters.Parse(connectionParameters);
        var client = CreateClient(config);

        var payload = new PlugV2CotationRequest
        {
            BrokerCnpj = request.BrokerCnpj,
            PolicyHolderCnpj = request.PolicyHolderCnpj,
            InsuredCpfCnpj = request.InsuredCpfCnpj,
            InsuranceUniqueId = request.InsuranceUniqueId,
            ModalityGlobalId = request.ModalityGlobalId,
            ModalityName = request.ModalityName,
            InsuredAmountValue = request.InsuredAmount,
            StartDate = request.StartDate.ToDateTime(TimeOnly.MinValue),
            EndDate = request.EndDate.ToDateTime(TimeOnly.MinValue),
            AdditionalCoverages = request.AdditionalCoverages,
        };

        try
        {
            var body = await PostJsonAsync(client, "Cotation", config.Key, payload, cancellationToken);

            var envelope = JsonSerializer.Deserialize<PlugV2CotationResponse>(body, JsonOptions);

            if (envelope?.Response is null || envelope.HasError)
            {
                throw new CalculationEngineException(FailureMessage("solicitar Cotação", body));
            }

            return PlugV2QuotationAclMapper.Map(envelope.Response);
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
            throw new CalculationEngineException("Falha ao solicitar Cotação no motor PlugV2.", exception);
        }
    }

    /// <summary>
    /// RN-063 ("Baixar minuta", parte 1): envia os termos preenchidos (Tags do objeto + Cláusulas
    /// particulares marcadas) da proposta selecionada (POST /UpdateProposalTerms).
    /// </summary>
    public async Task SubmitProposalTermsAsync(
        string? connectionParameters, SubmitProposalTermsInput request, CancellationToken cancellationToken)
    {
        var config = PlugV2ConnectionParameters.Parse(connectionParameters);
        var client = CreateClient(config);

        var payload = new PlugV2UpdateProposalTermsRequest
        {
            BrokerCnpj = request.BrokerCnpj,
            ProposalUniqueId = request.ProposalExternalId,
            Terms = request.Terms
                .Select(term => new PlugV2ProposalTerm { Name = term.Name, Values = term.Value })
                .ToList(),
            ParticularClauses = request.ParticularClauses
                .Select(clause => new PlugV2ProposalParticularClause
                {
                    ParticularClauseId = clause.ParticularClauseId,
                    Tags = clause.Tags
                        .Select(tag => new PlugV2ProposalTerm { Name = tag.Name, Values = tag.Value })
                        .ToList(),
                })
                .ToList(),
        };

        try
        {
            var body = await PostJsonAsync(client, "UpdateProposalTerms", config.Key, payload, cancellationToken);

            var envelope = JsonSerializer.Deserialize<PlugV2ErrorEnvelope>(body, JsonOptions);
            if (envelope is { HasError: true })
            {
                throw new CalculationEngineException(FailureMessage("enviar os termos da proposta", body));
            }
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
            throw new CalculationEngineException("Falha ao enviar os termos da proposta no motor PlugV2.", exception);
        }
    }

    /// <summary>RN-063 ("Baixar minuta", parte 2): obtém a minuta (documento) da proposta (POST /GetProposalContractDraft).</summary>
    public async Task<ProposalContractDraftResult> GetProposalContractDraftAsync(
        string? connectionParameters, string brokerCnpj, string proposalExternalId, CancellationToken cancellationToken)
    {
        var config = PlugV2ConnectionParameters.Parse(connectionParameters);
        var client = CreateClient(config);

        var payload = new PlugV2GetProposalContractDraftRequest
        {
            BrokerCnpj = brokerCnpj,
            ProposalUniqueId = proposalExternalId,
        };

        try
        {
            var body = await PostJsonAsync(client, "GetProposalContractDraft", config.Key, payload, cancellationToken);

            var envelope = JsonSerializer.Deserialize<PlugV2ProposalContractDraftResponse>(body, JsonOptions);
            if (envelope?.Response is null || envelope.HasError)
            {
                throw new CalculationEngineException(FailureMessage("obter a minuta da proposta", body));
            }

            var draft = envelope.Response;
            return new ProposalContractDraftResult(draft.Url, draft.UniqueId, draft.CreateDate);
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
            throw new CalculationEngineException("Falha ao obter a minuta da proposta no motor PlugV2.", exception);
        }
    }

    private HttpClient CreateClient(PlugV2ConnectionParameters config)
    {
        var client = httpClientFactory.CreateClient(ClientName);
        // Barra final preserva o caminho do gateway (ex.: /qa/garantia/api/PlugV2) na resolução relativa.
        client.BaseAddress = new Uri(config.BaseUrl.EndsWith('/') ? config.BaseUrl : config.BaseUrl + "/");
        client.Timeout = TimeSpan.FromSeconds(TimeoutSeconds);
        return client;
    }

    /// <summary>POST JSON no gateway com o header de chave; sobe CalculationEngineException em status HTTP de erro.</summary>
    private static async Task<string> PostJsonAsync(
        HttpClient client, string route, string key, object payload, CancellationToken cancellationToken)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, route)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(payload, JsonOptions),
                System.Text.Encoding.UTF8,
                "application/json"),
        };

        httpRequest.Headers.Add("application-key-v2", key);

        using var httpResponse = await client.SendAsync(httpRequest, cancellationToken);
        var body = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

        if (!httpResponse.IsSuccessStatusCode)
        {
            var errors = ExtractErrors(body);
            var suffix = string.IsNullOrEmpty(errors) ? string.Empty : $" {errors}";
            throw new CalculationEngineException(
                $"PlugV2 retornou status {(int)httpResponse.StatusCode} ({httpResponse.StatusCode}) em {route}.{suffix}");
        }

        return body;
    }

    /// <summary>Monta a mensagem de falha a partir do envelope de erro do gateway (Errors), quando presente.</summary>
    private static string FailureMessage(string action, string body)
    {
        var errors = ExtractErrors(body);
        return string.IsNullOrEmpty(errors)
            ? $"PlugV2 devolveu erro ao {action}."
            : $"PlugV2 devolveu erro ao {action}. {errors}";
    }

    private static string ExtractErrors(string? body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return string.Empty;
        }

        try
        {
            var envelope = JsonSerializer.Deserialize<PlugV2ErrorEnvelope>(body, JsonOptions);
            var errors = (envelope?.Errors ?? [])
                .Where(error => !string.IsNullOrWhiteSpace(error))
                .ToList();
            return errors.Count > 0 ? string.Join("; ", errors) : string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }
}

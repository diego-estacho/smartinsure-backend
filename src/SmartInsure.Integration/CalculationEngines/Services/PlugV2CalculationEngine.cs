using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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
    IHttpClientFactory httpClientFactory,
    IOptions<PlugV2Options> options,
    IQuotationIntegrationLogRecorder integrationLogRecorder) : ICalculationEngine
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private const string ClientName = "PlugV2";

    /// <summary>Client das chamadas NÃO idempotentes (/Cotation, /UpdateProposalTerms): registrado SEM retry (RN-057, ver DependencyInjection).</summary>
    public const string NonIdempotentClientName = "PlugV2-nonidempotent";

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

        // Embora seja leitura, o gateway trata a consulta de limites como uma "consulta" com dedup de
        // 60s (broker+seguradora+tomador): um retry no timeout re-dispara e cai em "Já existe uma
        // consulta para este CNPJ" (400). Por isso vai no client SEM retry (tentativa única, timeout
        // largo), igual às chamadas mutantes — não na resiliência padrão (plugv2-dedup).
        var client = CreateNonIdempotentClient(config);

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

            var responseContent = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

            if (!httpResponse.IsSuccessStatusCode)
            {
                // Inclui o motivo do gateway (Errors do envelope) — um "BadRequest" cru não diz nada ao
                // usuário; a RN-030 apresenta este motivo por Seguradora. Consistente com as demais chamadas.
                var reason = ExtractErrors(responseContent);
                var suffix = string.IsNullOrEmpty(reason) ? string.Empty : $" {reason}";
                throw new CalculationEngineException(
                    $"PlugV2 retornou status {(int)httpResponse.StatusCode} ({httpResponse.StatusCode}) na consulta de limites de crédito.{suffix}");
            }

            var response = JsonSerializer.Deserialize<PlugV2GetPolicyHolderLimitsAndRatesResponse>(
                responseContent, JsonOptions);

            // RN-030: resposta nula/vazia ou com erro => indisponível (null Response também, não só lista vazia).
            if (response is null || response.HasError || response.Response is null || response.Response.Count == 0)
            {
                return null;
            }

            // Localizar resposta da Seguradora pelo InsuranceUniqueId (case-insensitive).
            var insurerResponse = response.Response.FirstOrDefault(r =>
                r.Insurance?.InsuranceUniqueId?.Equals(insurerExternalId, StringComparison.OrdinalIgnoreCase) == true);

            // RN-030: Seguradora não encontrada/ sem linhas na resposta => indisponível.
            if (insurerResponse is null || insurerResponse.LimitsAndRates is null || insurerResponse.LimitsAndRates.Count == 0)
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
        // RN-057: /Cotation CRIA a proposta → client sem retry. Repetir a chamada re-dispara o create e o
        // gateway responde "já existe uma cotação" (dedup de 60s). Ver CreateNonIdempotentClient.
        var client = CreateNonIdempotentClient(config);

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
            // O gateway PlugV2 só inclui o PolicyHolderCCG (veredito de CCG) quando EmissionProposalType
            // == 2 (InsurePoint) — confirmado no OnPoint-Backend (BuildCotationResponse) e no probe ao
            // vivo. Enviamos 2 para receber o CCG, como a plataforma legada. Trocar se o gateway passar a
            // reconhecer um tipo próprio do SmartInsure.
            EmissionProposalType = 2,
        };

        // ADR-102: request/response gravados no QuotationIntegrationLog — só o corpo, nunca headers/segredo.
        var requestJson = JsonSerializer.Serialize(payload, JsonOptions);
        var stopwatch = Stopwatch.StartNew();
        string? capturedBody = null;
        int? capturedHttpStatus = null;

        try
        {
            var (status, body) = await PostJsonNoThrowAsync(client, "Cotation", config.Key, payload, cancellationToken);
            capturedHttpStatus = (int)status;
            capturedBody = body;

            if (!IsSuccessStatusCode(status))
            {
                var message = BuildStatusErrorMessage("Cotation", status, body);
                await RecordAsync(request, requestJson, body, (int)status, QuotationIntegrationOutcome.Failed,
                    quotationStatus: null, errorMessage: message, stopwatch.ElapsedMilliseconds, cancellationToken);

                throw new CalculationEngineException(message);
            }

            var envelope = JsonSerializer.Deserialize<PlugV2CotationResponse>(body, JsonOptions);

            // Sem payload utilizável (falha de transporte/gateway sem corpo) → falha de integração (RN-057).
            if (envelope?.Response is null)
            {
                var message = FailureMessage("solicitar Cotação", body);
                await RecordAsync(request, requestJson, body, (int)status, QuotationIntegrationOutcome.Failed,
                    quotationStatus: null, errorMessage: message, stopwatch.ElapsedMilliseconds, cancellationToken);

                throw new CalculationEngineException(message);
            }

            // ADR-064: a ACL é o ÚNICO ponto de classificação. Passamos o sinal de erro do envelope e seus
            // motivos: quando HasError, a ACL classifica Indisponível-com-motivos (nunca seguível); caso
            // contrário, classifica o payload normalmente. Nunca confia num status/prêmio de payload errado.
            var result = PlugV2QuotationAclMapper.Map(envelope.Response, envelope.HasError, envelope.Errors);

            await RecordAsync(request, requestJson, body, (int)status, QuotationIntegrationOutcome.Completed,
                quotationStatus: result.Result.ToString(), errorMessage: null, stopwatch.ElapsedMilliseconds, cancellationToken);

            return result;
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
            await RecordAsync(request, requestJson, capturedBody, capturedHttpStatus, QuotationIntegrationOutcome.Failed,
                quotationStatus: null, errorMessage: exception.Message, stopwatch.ElapsedMilliseconds, cancellationToken);

            throw new CalculationEngineException("Falha ao solicitar Cotação no motor PlugV2.", exception);
        }
    }

    /// <summary>Grava o log de integração da Cotação (ADR-102) — best-effort por contrato do recorder; nunca deve subir para aqui.</summary>
    private Task RecordAsync(
        QuotationRequestInput request,
        string requestJson,
        string? responseBody,
        int? httpStatus,
        string outcome,
        string? quotationStatus,
        string? errorMessage,
        long durationMs,
        CancellationToken cancellationToken)
        => integrationLogRecorder.RecordCotationAsync(
            new QuotationIntegrationLogContext
            {
                QuotationId = request.QuotationId,
                QuotationGroupId = request.QuotationGroupId,
                InsurerId = request.InsurerId,
                EngineType = nameof(ECalculationEngine.PlugV2),
                Outcome = outcome,
                QuotationStatus = quotationStatus,
                DurationMs = durationMs,
                RequestPayload = requestJson,
                ResponseRaw = responseBody,
                HttpStatus = httpStatus,
                ErrorMessage = errorMessage,
            },
            cancellationToken);

    /// <summary>
    /// RN-080 ("Baixar minuta", parte 1): envia os termos preenchidos (Tags do objeto + Cláusulas
    /// particulares marcadas) da proposta selecionada (POST /UpdateProposalTerms).
    /// </summary>
    public async Task SubmitProposalTermsAsync(
        string? connectionParameters, SubmitProposalTermsInput request, CancellationToken cancellationToken)
    {
        var config = PlugV2ConnectionParameters.Parse(connectionParameters);
        // Mutação de proposta → também sem retry (mesma razão do /Cotation, RN-057).
        var client = CreateNonIdempotentClient(config);

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

    /// <summary>RN-080 ("Baixar minuta", parte 2): obtém a minuta (documento) da proposta (POST /GetProposalContractDraft).</summary>
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

    /// <summary>
    /// Client das chamadas que o gateway DEDUPA — as mutantes (/Cotation, /UpdateProposalTerms) e a
    /// consulta de limites (leitura, mas dedupada como "consulta"). Registrado SEM resiliência de retry
    /// (RN-057): re-tentar re-dispara e cai no dedup ("já existe"). Tentativa única, com timeout largo e configurável
    /// (<see cref="PlugV2Options.NonIdempotentTimeoutSeconds"/>) para acomodar a latência real numa só ida.
    /// </summary>
    private HttpClient CreateNonIdempotentClient(PlugV2ConnectionParameters config)
    {
        var client = httpClientFactory.CreateClient(NonIdempotentClientName);
        client.BaseAddress = new Uri(config.BaseUrl.EndsWith('/') ? config.BaseUrl : config.BaseUrl + "/");
        client.Timeout = TimeSpan.FromSeconds(options.Value.NonIdempotentTimeoutSeconds);
        return client;
    }

    /// <summary>POST JSON no gateway com o header de chave; sobe CalculationEngineException em status HTTP de erro.</summary>
    private static async Task<string> PostJsonAsync(
        HttpClient client, string route, string key, object payload, CancellationToken cancellationToken)
    {
        var (status, body) = await PostJsonNoThrowAsync(client, route, key, payload, cancellationToken);

        if (!IsSuccessStatusCode(status))
        {
            throw new CalculationEngineException(BuildStatusErrorMessage(route, status, body));
        }

        return body;
    }

    /// <summary>
    /// Variante de <see cref="PostJsonAsync"/> que NUNCA lança por status HTTP de erro — devolve o status e o
    /// corpo exatos para o chamador registrar (ADR-102: RunQuotationAsync loga o status em sucesso E falha)
    /// e classificar. Falha de transporte ainda sobe (conexão recusada, timeout, etc.).
    /// </summary>
    private static async Task<(HttpStatusCode Status, string Body)> PostJsonNoThrowAsync(
        HttpClient client, string route, string key, object payload, CancellationToken cancellationToken)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, route)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(payload, JsonOptions),
                Encoding.UTF8,
                "application/json"),
        };

        httpRequest.Headers.Add("application-key-v2", key);

        using var httpResponse = await client.SendAsync(httpRequest, cancellationToken);
        var body = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

        return (httpResponse.StatusCode, body);
    }

    /// <summary>Mesmo critério de <see cref="HttpResponseMessage.IsSuccessStatusCode"/>, sobre o HttpStatusCode isolado.</summary>
    private static bool IsSuccessStatusCode(HttpStatusCode status) => (int)status is >= 200 and <= 299;

    private static string BuildStatusErrorMessage(string route, HttpStatusCode status, string body)
    {
        var errors = ExtractErrors(body);
        var suffix = string.IsNullOrEmpty(errors) ? string.Empty : $" {errors}";
        return $"PlugV2 retornou status {(int)status} ({status}) em {route}.{suffix}";
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

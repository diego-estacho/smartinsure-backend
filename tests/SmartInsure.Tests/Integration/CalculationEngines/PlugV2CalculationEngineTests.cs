using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Exceptions;
using SmartInsure.Core.Enumerators;
using SmartInsure.Integration.CalculationEngines;
using SmartInsure.Integration.CalculationEngines.PlugV2;
using SmartInsure.Integration.CalculationEngines.Services;

namespace SmartInsure.Tests.Integration.CalculationEngines;

/// <summary>RN-029/RN-030 — Motor PlugV2: consulta limites de crédito com tratamento de erros e validação de payload.</summary>
[Trait("RuleId", "RN-029")]
[Trait("RuleId", "RN-030")]
public class PlugV2CalculationEngineTests
{
    private static readonly string BrokerageCnpj = "12345678000195";
    private static readonly string PolicyHolderCnpj = "98765432000109";
    private static readonly string InsurerExternalId = "insurer-id-123";
    private static readonly string ConnectionParameters = """{"baseUrl":"https://plug.example.com","key":"test-key"}""";

    /// <summary>Factory fake para injetar nosso HttpClient.</summary>
    private class FakeHttpClientFactory : IHttpClientFactory
    {
        private readonly HttpClient _client;

        public FakeHttpClientFactory(HttpClient client)
        {
            _client = client;
        }

        public HttpClient CreateClient(string name) => _client;
    }

    /// <summary>Fake HTTP handler que captura a requisição para validação.</summary>
    private class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, Task<HttpResponseMessage>> _handler;
        public HttpRequestMessage? CapturedRequest { get; private set; }
        public string? CapturedRequestBody { get; private set; }

        public FakeHttpMessageHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> handler)
        {
            _handler = handler;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            CapturedRequest = request;
            // Read the body while it's still available
            if (request.Content is not null)
            {
                CapturedRequestBody = await request.Content.ReadAsStringAsync(cancellationToken);
            }
            return await _handler(request);
        }
    }

    private PlugV2CalculationEngine BuildEngine(FakeHttpMessageHandler fakeHandler)
        => BuildEngine(fakeHandler, Substitute.For<IQuotationIntegrationLogRecorder>());

    /// <summary>Overload usado pelos testes de ADR-102 que precisam inspecionar as chamadas ao recorder.</summary>
    private PlugV2CalculationEngine BuildEngine(
        FakeHttpMessageHandler fakeHandler, IQuotationIntegrationLogRecorder integrationLogRecorder)
    {
        var services = new ServiceCollection();
        var httpClient = new HttpClient(fakeHandler);
        services.AddSingleton<IHttpClientFactory>(new FakeHttpClientFactory(httpClient));

        // O engine passou a depender de IOptions<PlugV2Options> (timeout das chamadas não idempotentes).
        services.AddOptions<PlugV2Options>();

        // ADR-102: log de integração da Cotação — substituto por padrão, injetado explicitamente nos
        // testes que verificam a gravação.
        services.AddSingleton(integrationLogRecorder);

        // Register the engine
        services.AddKeyedScoped<ICalculationEngine, PlugV2CalculationEngine>(
            ECalculationEngine.PlugV2);

        var provider = services.BuildServiceProvider();
        return (PlugV2CalculationEngine)provider.GetRequiredKeyedService<ICalculationEngine>(
            ECalculationEngine.PlugV2);
    }

    [Fact]
    public async Task GetPolicyHolderLimitsAndRatesAsync_DeveMapearResposta_QuandoRetorna200ComJsonValido()
    {
        var responseJson = new
        {
            StatusCode = 200,
            HasError = false,
            Errors = new object[] { },
            Response = new[]
            {
                new
                {
                    Insurance = new
                    {
                        Id = 325,
                        Name = "Essor Seguros S.A.",
                        InsuranceUniqueId = InsurerExternalId,
                    },
                    PolicyHolderName = "SPAL",
                    PolicyHolderCnpj = PolicyHolderCnpj,
                    PolicyHolderUniqueId = "338b04ff-1234-5678-abcd-ef0123456789",
                    CanSetupAProposal = true,
                    LimitsAndRates = new[]
                    {
                        new
                        {
                            BranchName = "Garantia",
                            BranchCode = "76",
                            ModalityGroupName = "Tradicional",
                            ModalityGroupType = "GARANTIA_TRADICIONAL",
                            ModalityName = "Tradicional",
                            ModalityUniqueId = "1a44d0ef-1234-5678-abcd-ef0123456789",
                            LimitRevised = 1000m,
                            AvailableLimit = 1000m,
                            Tax = 0.05m,
                        },
                    },
                }
            },
        };

        var fakeHandler = new FakeHttpMessageHandler(async request =>
        {
            await Task.Delay(0); // Simulate async work
            return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(responseJson, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    }),
                    System.Text.Encoding.UTF8,
                    "application/json"),
            };
        });

        var engine = BuildEngine(fakeHandler);
        var result = await engine.GetPolicyHolderLimitsAndRatesAsync(
            ConnectionParameters, BrokerageCnpj, PolicyHolderCnpj, InsurerExternalId,
            CancellationToken.None);

        result.Should().NotBeNull();
        result!.Groups.Should().HaveCount(1);
        result.Groups[0].GroupName.Should().Be("Tradicional");
        result.Groups[0].AvailableLimit.Should().Be(1000m);
        result.Groups[0].RevisedLimit.Should().Be(1000m);
        result.Groups[0].Rate.Should().Be(0.05m);
    }

    [Fact]
    public async Task GetPolicyHolderLimitsAndRatesAsync_DeveEnviarHeaderAutenticacao_QuandoRequisicaoFoiFeita()
    {
        var fakeHandler = new FakeHttpMessageHandler(async request =>
        {
            await Task.Delay(0);
            return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(new { StatusCode = 200, HasError = false, Errors = new object[] { }, Response = new object[] { } }),
                    System.Text.Encoding.UTF8,
                    "application/json"),
            };
        });

        var engine = BuildEngine(fakeHandler);
        await engine.GetPolicyHolderLimitsAndRatesAsync(
            ConnectionParameters, BrokerageCnpj, PolicyHolderCnpj, InsurerExternalId,
            CancellationToken.None);

        fakeHandler.CapturedRequest.Should().NotBeNull();
        fakeHandler.CapturedRequest!.Headers.Should().Contain(h =>
            h.Key == "application-key-v2" && h.Value.Contains("test-key"));
    }

    [Fact]
    public async Task GetPolicyHolderLimitsAndRatesAsync_DeveEnviarBodyComDadosCorretos_QuandoRequisicaoFoiFeita()
    {
        var fakeHandler = new FakeHttpMessageHandler(async request =>
        {
            await Task.Delay(0);
            return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(new { StatusCode = 200, HasError = false, Errors = new object[] { }, Response = new object[] { } }),
                    System.Text.Encoding.UTF8,
                    "application/json"),
            };
        });

        var engine = BuildEngine(fakeHandler);
        await engine.GetPolicyHolderLimitsAndRatesAsync(
            ConnectionParameters, BrokerageCnpj, PolicyHolderCnpj, InsurerExternalId,
            CancellationToken.None);

        fakeHandler.CapturedRequest.Should().NotBeNull();
        fakeHandler.CapturedRequestBody.Should().NotBeNull();
        var bodyJson = JsonSerializer.Deserialize<JsonElement>(fakeHandler.CapturedRequestBody!);

        // The request uses PascalCase due to [JsonPropertyName] attributes
        bodyJson.GetProperty("BrokerCnpj").GetString().Should().Be(BrokerageCnpj);
        bodyJson.GetProperty("PolicyHolderCnpj").GetString().Should().Be(PolicyHolderCnpj);
        bodyJson.GetProperty("InsuranceUniqueId").GetString().Should().Be(InsurerExternalId);
    }

    [Fact]
    public async Task GetPolicyHolderLimitsAndRatesAsync_DeveRetornarNull_QuandoRespostaComSuccessFalse()
    {
        var fakeHandler = new FakeHttpMessageHandler(async request =>
        {
            await Task.Delay(0);
            return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(new { StatusCode = 200, HasError = true, Errors = new[] { "Not found" }, Response = (object[])null }),
                    System.Text.Encoding.UTF8,
                    "application/json"),
            };
        });

        var engine = BuildEngine(fakeHandler);
        var result = await engine.GetPolicyHolderLimitsAndRatesAsync(
            ConnectionParameters, BrokerageCnpj, PolicyHolderCnpj, InsurerExternalId,
            CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetPolicyHolderLimitsAndRatesAsync_DeveRetornarNull_QuandoRespostaSerializadaComNull()
    {
        var fakeHandler = new FakeHttpMessageHandler(async request =>
        {
            await Task.Delay(0);
            return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(
                    "null",
                    System.Text.Encoding.UTF8,
                    "application/json"),
            };
        });

        var engine = BuildEngine(fakeHandler);
        var result = await engine.GetPolicyHolderLimitsAndRatesAsync(
            ConnectionParameters, BrokerageCnpj, PolicyHolderCnpj, InsurerExternalId,
            CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetPolicyHolderLimitsAndRatesAsync_DeveLancarCalculationEngineException_QuandoHttp500()
    {
        var fakeHandler = new FakeHttpMessageHandler(async request =>
        {
            await Task.Delay(0);
            return new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError)
            {
                Content = new StringContent("Internal Server Error"),
            };
        });

        var engine = BuildEngine(fakeHandler);
        var act = () => engine.GetPolicyHolderLimitsAndRatesAsync(
            ConnectionParameters, BrokerageCnpj, PolicyHolderCnpj, InsurerExternalId,
            CancellationToken.None);

        (await act.Should().ThrowAsync<CalculationEngineException>())
            .WithMessage("*PlugV2 retornou status*");
    }

    [Fact]
    public async Task GetPolicyHolderLimitsAndRatesAsync_DeveLancarCalculationEngineException_QuandoJsonInvalido()
    {
        var fakeHandler = new FakeHttpMessageHandler(async request =>
        {
            await Task.Delay(0);
            return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(
                    "<html>Internal Server Error</html>", // Invalid JSON
                    System.Text.Encoding.UTF8,
                    "application/json"),
            };
        });

        var engine = BuildEngine(fakeHandler);
        var act = () => engine.GetPolicyHolderLimitsAndRatesAsync(
            ConnectionParameters, BrokerageCnpj, PolicyHolderCnpj, InsurerExternalId,
            CancellationToken.None);

        (await act.Should().ThrowAsync<CalculationEngineException>())
            .WithMessage("*Falha ao consultar limites de crédito*");
    }

    [Fact]
    public async Task GetPolicyHolderLimitsAndRatesAsync_DeveLancarCalculationEngineException_QuandoJsonMissingRequiredField()
    {
        var fakeHandler = new FakeHttpMessageHandler(async request =>
        {
            await Task.Delay(0);
            // Missing required fields like StatusCode, Response, etc.
            var invalidJson = new { message = "Missing fields" };
            return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(invalidJson),
                    System.Text.Encoding.UTF8,
                    "application/json"),
            };
        });

        var engine = BuildEngine(fakeHandler);
        var act = () => engine.GetPolicyHolderLimitsAndRatesAsync(
            ConnectionParameters, BrokerageCnpj, PolicyHolderCnpj, InsurerExternalId,
            CancellationToken.None);

        (await act.Should().ThrowAsync<CalculationEngineException>())
            .WithMessage("*Falha ao consultar limites de crédito*");
    }

    [Fact]
    public async Task GetPolicyHolderLimitsAndRatesAsync_DevePermitirModalidadesOpcionaisNaResposta_QuandoAlgunsLimitesAusentes()
    {
        var responseJson = new
        {
            StatusCode = 200,
            HasError = false,
            Errors = new object[] { },
            Response = new[]
            {
                new
                {
                    Insurance = new
                    {
                        Id = 325,
                        Name = "Essor Seguros S.A.",
                        InsuranceUniqueId = InsurerExternalId,
                    },
                    PolicyHolderName = "SPAL",
                    PolicyHolderCnpj = PolicyHolderCnpj,
                    PolicyHolderUniqueId = "338b04ff-1234-5678-abcd-ef0123456789",
                    CanSetupAProposal = true,
                    LimitsAndRates = new[]
                    {
                        new
                        {
                            BranchName = "Garantia",
                            BranchCode = "76",
                            ModalityGroupName = "Tradicional",
                            ModalityGroupType = "GARANTIA_TRADICIONAL",
                            ModalityName = "Tradicional",
                            ModalityUniqueId = "1a44d0ef-1234-5678-abcd-ef0123456789",
                            LimitRevised = 1000m,
                            AvailableLimit = 1000m,
                            Tax = 0.05m,
                        },
                    },
                }
            },
        };

        var fakeHandler = new FakeHttpMessageHandler(async request =>
        {
            await Task.Delay(0);
            return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(responseJson, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    }),
                    System.Text.Encoding.UTF8,
                    "application/json"),
            };
        });

        var engine = BuildEngine(fakeHandler);
        var result = await engine.GetPolicyHolderLimitsAndRatesAsync(
            ConnectionParameters, BrokerageCnpj, PolicyHolderCnpj, InsurerExternalId,
            CancellationToken.None);

        result.Should().NotBeNull();
        result!.Groups.Should().HaveCount(1);
        result.Groups[0].GroupName.Should().Be("Tradicional");
        result.Groups[0].AvailableLimit.Should().Be(1000m);
    }

    [Fact]
    public async Task GetPolicyHolderLimitsAndRates_DeveRetornarNulo_QuandoResponseVazio()
    {
        // RN-030: retorno sem nenhuma seguradora vira indisponibilidade, nunca resultado válido.
        var fakeHandler = new FakeHttpMessageHandler(request =>
            Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(new { StatusCode = 200, HasError = false, Errors = new object[] { }, Response = new object[] { } }),
                    System.Text.Encoding.UTF8,
                    "application/json"),
            }));

        var engine = BuildEngine(fakeHandler);
        var result = await engine.GetPolicyHolderLimitsAndRatesAsync(
            ConnectionParameters, BrokerageCnpj, PolicyHolderCnpj, InsurerExternalId,
            CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetPolicyHolderLimitsAndRatesAsync_DeveSetarBaseUrl_DaConfiguracao_NaRequisicao()
    {
        var fakeHandler = new FakeHttpMessageHandler(async request =>
        {
            await Task.Delay(0);
            return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(new { StatusCode = 200, HasError = false, Errors = new object[] { }, Response = new object[] { } }),
                    System.Text.Encoding.UTF8,
                    "application/json"),
            };
        });

        var engine = BuildEngine(fakeHandler);
        await engine.GetPolicyHolderLimitsAndRatesAsync(
            ConnectionParameters, BrokerageCnpj, PolicyHolderCnpj, InsurerExternalId,
            CancellationToken.None);

        fakeHandler.CapturedRequest.Should().NotBeNull();
        // The request URI should use the base URL from connection parameters
        fakeHandler.CapturedRequest!.RequestUri.Should().NotBeNull();
        fakeHandler.CapturedRequest!.RequestUri.ToString()
            .Should().Contain("plug.example.com");
    }

    [Fact]
    [Trait("RuleId", "RN-029")]
    public async Task GetPolicyHolderLimitsAndRates_DeveAgregarPeloMaiorLimiteDisponivel_QuandoGrupoTemVariasModalidades()
    {
        // RN-029: valor do grupo = maior limite entre as modalidades; taxa e revisado vêm da MESMA linha.
        var responseJson = new
        {
            StatusCode = 200,
            HasError = false,
            Errors = new object[] { },
            Response = new[]
            {
                new
                {
                    Insurance = new { Id = 325, Name = "Essor Seguros S.A.", InsuranceUniqueId = InsurerExternalId },
                    PolicyHolderName = "SPAL",
                    PolicyHolderCnpj = PolicyHolderCnpj,
                    PolicyHolderUniqueId = "338b04ff-1234-5678-abcd-ef0123456789",
                    CanSetupAProposal = true,
                    LimitsAndRates = new[]
                    {
                        new { BranchName = "Setor Privado", BranchCode = "76", ModalityGroupName = "Tradicional", ModalityGroupType = "GARANTIA_TRADICIONAL", ModalityName = "Licitante", ModalityUniqueId = "m1", LimitRevised = 3_000_000m, AvailableLimit = 2_000_000m, Tax = 0.50m },
                        new { BranchName = "Setor Público", BranchCode = "75", ModalityGroupName = "Tradicional", ModalityGroupType = "GARANTIA_TRADICIONAL", ModalityName = "Executante Construtor", ModalityUniqueId = "m2", LimitRevised = 5_000_000m, AvailableLimit = 4_500_000m, Tax = 0.43m },
                        new { BranchName = "Setor Público", BranchCode = "75", ModalityGroupName = "Tradicional", ModalityGroupType = "GARANTIA_TRADICIONAL", ModalityName = "Retenção", ModalityUniqueId = "m3", LimitRevised = 1_000_000m, AvailableLimit = 900_000m, Tax = 0.60m },
                        new { BranchName = "Setor Público", BranchCode = "75", ModalityGroupName = "Judiciais", ModalityGroupType = "GARANTIA_JUDICIAIS", ModalityName = "Execução Trabalhista", ModalityUniqueId = "m4", LimitRevised = 1_000_000m, AvailableLimit = 800_000m, Tax = 0.97m },
                    },
                },
            },
        };

        var fakeHandler = new FakeHttpMessageHandler(request =>
            Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(responseJson),
                    System.Text.Encoding.UTF8,
                    "application/json"),
            }));

        var engine = BuildEngine(fakeHandler);
        var result = await engine.GetPolicyHolderLimitsAndRatesAsync(
            ConnectionParameters, BrokerageCnpj, PolicyHolderCnpj, InsurerExternalId,
            CancellationToken.None);

        result.Should().NotBeNull();
        result!.Groups.Should().HaveCount(2);

        var traditional = result.Groups.Single(g => g.GroupName == "Tradicional");
        traditional.AvailableLimit.Should().Be(4_500_000m);
        traditional.RevisedLimit.Should().Be(5_000_000m);
        traditional.Rate.Should().Be(0.43m);

        var judicial = result.Groups.Single(g => g.GroupName == "Judiciais");
        judicial.AvailableLimit.Should().Be(800_000m);
        judicial.Rate.Should().Be(0.97m);
    }

    /// <summary>Handler que conta as invocações — prova a ausência de retry no client não idempotente.</summary>
    private sealed class CountingHandler(HttpStatusCode status) : HttpMessageHandler
    {
        public int Count { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Count++;
            return Task.FromResult(new HttpResponseMessage(status)
            {
                Content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json"),
            });
        }
    }

    [Fact]
    [Trait("RuleId", "RN-057")]
    public async Task ClienteNaoIdempotente_NaoReTenta_MesmoComRespostaTransitoria()
    {
        // RN-057: /Cotation e /UpdateProposalTerms CRIAM/mutam recurso → jamais re-tentam. Re-tentar
        // re-dispararia o create e cairia no dedup de 60s do gateway ("já existe uma cotação"). O client
        // não idempotente é registrado SEM retry — ao contrário das leituras, com resiliência padrão.
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        services.AddCalculationEngines();

        var counter = new CountingHandler(HttpStatusCode.InternalServerError);
        services.AddHttpClient(PlugV2CalculationEngine.NonIdempotentClientName)
            .ConfigurePrimaryHttpMessageHandler(() => counter);

        using var provider = services.BuildServiceProvider();
        var client = provider.GetRequiredService<IHttpClientFactory>()
            .CreateClient(PlugV2CalculationEngine.NonIdempotentClientName);

        await client.PostAsync(
            "https://plug.example.com/Cotation",
            new StringContent("{}", System.Text.Encoding.UTF8, "application/json"),
            CancellationToken.None);

        // Exatamente 1 ida ao gateway — sem a 2ª tentativa que dispararia o "já existe".
        counter.Count.Should().Be(1);
    }

    [Fact]
    [Trait("RuleId", "RN-058")]
    public async Task RunQuotationAsync_EnviaEmissionProposalType2_ParaReceberOCcgDoGateway()
    {
        // O gateway PlugV2 só devolve o PolicyHolderCCG (veredito de CCG) quando EmissionProposalType == 2
        // (InsurePoint) — sem esse campo o CCG NUNCA vem (confirmado no OnPoint-Backend e no probe ao vivo).
        var fakeHandler = new FakeHttpMessageHandler(request =>
            Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(new { StatusCode = 200, HasError = false, Errors = new object[] { }, Response = new { ResponseStatus = new { Status = 1, Message = "ok" }, Success = true, InsurancePremium = 300m } }),
                    System.Text.Encoding.UTF8, "application/json"),
            }));

        var engine = BuildEngine(fakeHandler);
        var input = BuildQuotationRequestInput();

        await engine.RunQuotationAsync(ConnectionParameters, input, CancellationToken.None);

        fakeHandler.CapturedRequestBody.Should().NotBeNull();
        var body = JsonSerializer.Deserialize<JsonElement>(fakeHandler.CapturedRequestBody!);
        body.GetProperty("EmissionProposalType").GetInt32().Should().Be(2);
    }

    /// <summary>Monta um QuotationRequestInput válido para os testes de RunQuotationAsync — os 3 ids servem só ao log de integração (ADR-102).</summary>
    private static QuotationRequestInput BuildQuotationRequestInput() => new()
    {
        QuotationId = Guid.CreateVersion7(),
        QuotationGroupId = Guid.CreateVersion7(),
        InsurerId = Guid.CreateVersion7(),
        BrokerCnpj = BrokerageCnpj,
        PolicyHolderCnpj = PolicyHolderCnpj,
        InsuredCpfCnpj = PolicyHolderCnpj,
        InsuranceUniqueId = InsurerExternalId,
        ModalityGlobalId = "84",
        ModalityName = "Executante Construtor",
        InsuredAmount = 1_000_000m,
        StartDate = new DateOnly(2026, 8, 3),
        EndDate = new DateOnly(2027, 8, 3),
    };

    /// <summary>ADR-102 — a cada solicitação de Cotação (sucesso), o recorder grava Outcome=Completed com o status/HTTP/duração.</summary>
    [Fact]
    [Trait("RuleId", "RN-057")]
    public async Task RunQuotationAsync_RegistraLogDeIntegracaoCompleted_QuandoGatewayResponde200()
    {
        var fakeHandler = new FakeHttpMessageHandler(request =>
            Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(new { StatusCode = 200, HasError = false, Errors = new object[] { }, Response = new { ResponseStatus = new { Status = 1, Message = "ok" }, Success = true, InsurancePremium = 300m } }),
                    System.Text.Encoding.UTF8, "application/json"),
            }));

        var recorder = Substitute.For<IQuotationIntegrationLogRecorder>();
        QuotationIntegrationLogContext? captured = null;
        recorder.RecordCotationAsync(
                Arg.Do<QuotationIntegrationLogContext>(context => captured = context), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var engine = BuildEngine(fakeHandler, recorder);
        var input = BuildQuotationRequestInput();

        await engine.RunQuotationAsync(ConnectionParameters, input, CancellationToken.None);

        await recorder.Received(1).RecordCotationAsync(Arg.Any<QuotationIntegrationLogContext>(), Arg.Any<CancellationToken>());
        captured.Should().NotBeNull();
        captured!.Outcome.Should().Be(QuotationIntegrationOutcome.Completed);
        captured.QuotationId.Should().Be(input.QuotationId);
        captured.QuotationGroupId.Should().Be(input.QuotationGroupId);
        captured.InsurerId.Should().Be(input.InsurerId);
        captured.HttpStatus.Should().Be(200);
        captured.ErrorMessage.Should().BeNull();
        captured.RequestPayload.Should().Contain("InsuranceUniqueId");
        captured.ResponseRaw.Should().NotBeNullOrEmpty();
    }

    /// <summary>ADR-102 — em falha de gateway (400/500...), o recorder grava Outcome=Failed com o motivo, e a exceção original ainda sobe (RN-057, sem retry).</summary>
    [Fact]
    [Trait("RuleId", "RN-057")]
    public async Task RunQuotationAsync_RegistraLogDeIntegracaoFailed_QuandoGatewayRetorna400()
    {
        var fakeHandler = new FakeHttpMessageHandler(request =>
            Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(new { HasError = true, Errors = new[] { "já existe uma cotação" } }),
                    System.Text.Encoding.UTF8, "application/json"),
            }));

        var recorder = Substitute.For<IQuotationIntegrationLogRecorder>();
        QuotationIntegrationLogContext? captured = null;
        recorder.RecordCotationAsync(
                Arg.Do<QuotationIntegrationLogContext>(context => captured = context), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var engine = BuildEngine(fakeHandler, recorder);
        var input = BuildQuotationRequestInput();

        var act = () => engine.RunQuotationAsync(ConnectionParameters, input, CancellationToken.None);

        (await act.Should().ThrowAsync<CalculationEngineException>())
            .WithMessage("*já existe uma cotação*");

        await recorder.Received(1).RecordCotationAsync(Arg.Any<QuotationIntegrationLogContext>(), Arg.Any<CancellationToken>());
        captured.Should().NotBeNull();
        captured!.Outcome.Should().Be(QuotationIntegrationOutcome.Failed);
        captured.HttpStatus.Should().Be(400);
        captured.ErrorMessage.Should().Contain("já existe uma cotação");
    }
}

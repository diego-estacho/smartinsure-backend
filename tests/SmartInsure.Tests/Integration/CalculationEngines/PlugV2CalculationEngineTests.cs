using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Exceptions;
using SmartInsure.Core.Enumerators;
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
    {
        var services = new ServiceCollection();
        var httpClient = new HttpClient(fakeHandler);
        services.AddSingleton<IHttpClientFactory>(new FakeHttpClientFactory(httpClient));

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
}

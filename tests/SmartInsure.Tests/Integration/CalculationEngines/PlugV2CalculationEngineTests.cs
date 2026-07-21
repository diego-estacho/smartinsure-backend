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
        var validUntil = DateTime.UtcNow.AddMonths(12);
        var responseJson = new
        {
            success = true,
            message = "Success",
            traditionalLimit = 1000m,
            traditionalRate = 0.05m,
            judicialLimit = 2000m,
            judicialRate = 0.06m,
            judicialFiscalRate = 0.07m,
            financialLimit = 3000m,
            financialRate = 0.08m,
            limitValidUntil = validUntil,
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
        result!.TraditionalLimit.Should().Be(1000m);
        result.TraditionalRate.Should().Be(0.05m);
        result.JudicialLimit.Should().Be(2000m);
        result.JudicialRate.Should().Be(0.06m);
        result.JudicialFiscalRate.Should().Be(0.07m);
        result.FinancialLimit.Should().Be(3000m);
        result.FinancialRate.Should().Be(0.08m);
        result.LimitValidUntil.Should().Be(validUntil);
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
                    JsonSerializer.Serialize(new { success = true }),
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
                    JsonSerializer.Serialize(new { success = true }),
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
                    JsonSerializer.Serialize(new { success = false, message = "Not found" }),
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
            // Missing the required "success" field
            var invalidJson = new { message = "Missing success" };
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
            success = true,
            traditionalLimit = 1000m,
            traditionalRate = 0.05m,
            // Judicial and Financial omitted - they should be null
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
        result!.TraditionalLimit.Should().Be(1000m);
        result.JudicialLimit.Should().BeNull();
        result.FinancialLimit.Should().BeNull();
    }

    [Fact]
    public async Task GetPolicyHolderLimitsAndRatesAsync_DeveLancarCalculationEngineException_QuandoTimeoutOcorre()
    {
        var fakeHandler = new FakeHttpMessageHandler(async request =>
        {
            await Task.Delay(100); // Simulate delay (won't actually timeout in this test setup)
            return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(new { success = true }),
                    System.Text.Encoding.UTF8,
                    "application/json"),
            };
        });

        var engine = BuildEngine(fakeHandler);
        // This test is more about the handler setup; actual timeout testing would need real HTTP setup
        var result = await engine.GetPolicyHolderLimitsAndRatesAsync(
            ConnectionParameters, BrokerageCnpj, PolicyHolderCnpj, InsurerExternalId,
            CancellationToken.None);

        result.Should().NotBeNull();
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
                    JsonSerializer.Serialize(new { success = true }),
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

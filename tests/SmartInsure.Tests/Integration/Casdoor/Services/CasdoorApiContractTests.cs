using System.Net;
using System.Text;
using FluentAssertions;
using Refit;
using SmartInsure.Integration.Casdoor.Interfaces;

namespace SmartInsure.Tests.Integration.Casdoor.Services;

/// <summary>
/// RN-065 — contrato HTTP com o Casdoor. Substituto de <see cref="ICasdoorApi"/> não exercita os
/// atributos do Refit, então a query string é verificada aqui: é nela que mora a diferença entre
/// endereçar a identidade pelo UUID e pelo par <c>owner/name</c>.
/// </summary>
[Trait("RuleId", "RN-065")]
public class CasdoorApiContractTests
{
    private const string ExternalIdentity = "82796815-5b18-4e7f-ae6d-d91bd5e12ed8";

    [Fact]
    public async Task GetUser_DeveEnderecarPorUserId_QuandoRecebeUuidDaIdentidade()
    {
        var handler = new CapturingHandler();
        var api = CreateApi(handler);

        await api.GetUserAsync(ExternalIdentity, CancellationToken.None);

        // `id` no Casdoor é `owner/name`: com UUID puro a resposta é
        // "GetOwnerAndNameFromId() error, wrong token count for ID" e data nulo.
        handler.LastQuery.Should().Be($"?userId={ExternalIdentity}");
    }

    [Fact]
    public async Task SetPassword_DevePostarFormularioUrlEncoded_QuandoDefineSenha()
    {
        var handler = new CapturingHandler();
        var api = CreateApi(handler);

        await api.SetPasswordAsync(
            new Dictionary<string, string>
            {
                ["userOwner"] = "InsurePoint_DEV",
                ["userName"] = "dev_insp_maria_corretora_com_br",
                ["newPassword"] = "NovaSenha@1234",
            },
            CancellationToken.None);

        handler.LastPath.Should().Be("/api/set-password");
        handler.LastContentType.Should().Be("application/x-www-form-urlencoded");
        handler.LastBody.Should().Contain("userOwner=InsurePoint_DEV")
            .And.Contain("userName=dev_insp_maria_corretora_com_br");
    }

    private static ICasdoorApi CreateApi(CapturingHandler handler)
        => RestService.For<ICasdoorApi>(new HttpClient(handler)
        {
            BaseAddress = new Uri("https://sso.local"),
        });

    /// <summary>Registra a requisição de saída e responde um <c>ok</c> mínimo do Casdoor.</summary>
    private sealed class CapturingHandler : HttpMessageHandler
    {
        public string? LastPath { get; private set; }

        public string? LastQuery { get; private set; }

        public string? LastBody { get; private set; }

        public string? LastContentType { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastPath = request.RequestUri?.AbsolutePath;
            LastQuery = request.RequestUri?.Query;

            if (request.Content is not null)
            {
                LastBody = await request.Content.ReadAsStringAsync(cancellationToken);
                LastContentType = request.Content.Headers.ContentType?.MediaType;
            }

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """{"status":"ok","msg":"","data":null}""",
                    Encoding.UTF8,
                    "application/json"),
            };
        }
    }
}

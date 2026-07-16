using System.Net;
using FluentAssertions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Refit;
using SmartInsure.Core.Exceptions;
using SmartInsure.Integration.Casdoor.Interfaces;
using SmartInsure.Integration.Casdoor.Models;
using SmartInsure.Integration.Casdoor.Options;
using SmartInsure.Integration.Casdoor.Services;

namespace SmartInsure.Tests.Integration.Casdoor.Services;

/// <summary>RN-005 — validação de credenciais no provedor de identidade.</summary>
[Trait("RuleId", "RN-005")]
public class CasdoorIdentityProviderValidateCredentialsTests
{
    private const string Email = "maria@corretora.com.br";
    private const string Password = "senha-secreta";

    private readonly ICasdoorApi _api = Substitute.For<ICasdoorApi>();
    private readonly CasdoorIdentityProvider _provider;

    public CasdoorIdentityProviderValidateCredentialsTests()
        => _provider = new CasdoorIdentityProvider(_api, Options.Create(new CasdoorOptions
        {
            Domain = "https://sso.local",
            ClientId = "client-id",
            Secret = "client-secret",
            OrganizationName = "onpoint",
            AppName = "smartinsure",
            DefaultPassword = "senha-inicial",
            EnviromentUserCasdoor = "dev_insp",
        }));

    private void IdentityExists()
        => _api.GetUserByEmailAsync(Email, Arg.Any<CancellationToken>())
            .Returns(new CasdoorResponse<CasdoorUser?>
            {
                Status = "ok",
                Data = new CasdoorUser { Name = "dev_insp_maria_corretora_com_br" },
            });

    [Fact]
    public async Task ValidateCredentials_DeveRetornarTrue_QuandoProvedorEmiteToken()
    {
        IdentityExists();
        _api.RequestTokenAsync(Arg.Any<Dictionary<string, string>>(), Arg.Any<CancellationToken>())
            .Returns(new CasdoorTokenResponse { AccessToken = "token-casdoor" });

        var result = await _provider.ValidateCredentialsAsync(Email, Password, CancellationToken.None);

        result.Should().BeTrue();
        await _api.Received(1).RequestTokenAsync(
            Arg.Is<Dictionary<string, string>>(form =>
                form["grant_type"] == "password"
                && form["username"] == "dev_insp_maria_corretora_com_br"
                && form["password"] == Password),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ValidateCredentials_DeveRetornarFalse_QuandoProvedorRespondeErro()
    {
        IdentityExists();
        _api.RequestTokenAsync(Arg.Any<Dictionary<string, string>>(), Arg.Any<CancellationToken>())
            .Returns(new CasdoorTokenResponse { Error = "invalid_grant" });

        var result = await _provider.ValidateCredentialsAsync(Email, Password, CancellationToken.None);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateCredentials_DeveRetornarFalse_QuandoIdentidadeNaoExiste()
    {
        _api.GetUserByEmailAsync(Email, Arg.Any<CancellationToken>())
            .Returns(new CasdoorResponse<CasdoorUser?> { Status = "ok", Data = null });

        var result = await _provider.ValidateCredentialsAsync(Email, Password, CancellationToken.None);

        result.Should().BeFalse();
        await _api.DidNotReceive().RequestTokenAsync(
            Arg.Any<Dictionary<string, string>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ValidateCredentials_DeveRetornarFalse_QuandoProvedorRecusaCom4xx()
    {
        IdentityExists();
        _api.RequestTokenAsync(Arg.Any<Dictionary<string, string>>(), Arg.Any<CancellationToken>())
            .Returns<CasdoorTokenResponse>(_ => throw CreateApiException(HttpStatusCode.BadRequest));

        var result = await _provider.ValidateCredentialsAsync(Email, Password, CancellationToken.None);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateCredentials_DeveLancarIndisponibilidade_QuandoProvedorFora()
    {
        IdentityExists();
        _api.RequestTokenAsync(Arg.Any<Dictionary<string, string>>(), Arg.Any<CancellationToken>())
            .Returns<CasdoorTokenResponse>(_ => throw new HttpRequestException("connection refused"));

        var act = () => _provider.ValidateCredentialsAsync(Email, Password, CancellationToken.None);

        await act.Should().ThrowAsync<IdentityProviderUnavailableException>();
    }

    private static ApiException CreateApiException(HttpStatusCode statusCode)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Post, "https://sso.local/api/login/oauth/access_token");
        using var response = new HttpResponseMessage(statusCode);

        return ApiException.Create(request, HttpMethod.Post, response, new RefitSettings())
            .GetAwaiter().GetResult();
    }
}

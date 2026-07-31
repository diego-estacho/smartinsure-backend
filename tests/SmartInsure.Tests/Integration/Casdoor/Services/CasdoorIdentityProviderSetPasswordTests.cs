using FluentAssertions;
using Microsoft.Extensions.Options;
using NSubstitute;
using SmartInsure.Integration.Casdoor.Interfaces;
using SmartInsure.Integration.Casdoor.Models;
using SmartInsure.Integration.Casdoor.Options;
using SmartInsure.Integration.Casdoor.Services;

namespace SmartInsure.Tests.Integration.Casdoor.Services;

/// <summary>RN-065 — definição da senha no provedor de identidade durante o primeiro acesso.</summary>
[Trait("RuleId", "RN-065")]
public class CasdoorIdentityProviderSetPasswordTests
{
    private const string ExternalIdentity = "82796815-5b18-4e7f-ae6d-d91bd5e12ed8";
    private const string Organization = "InsurePoint_DEV";
    private const string Username = "dev_insp_maria_corretora_com_br";
    private const string NewPassword = "NovaSenha@1234";

    private readonly ICasdoorApi _api = Substitute.For<ICasdoorApi>();
    private readonly CasdoorIdentityProvider _provider;

    public CasdoorIdentityProviderSetPasswordTests()
        => _provider = new CasdoorIdentityProvider(_api, Options.Create(new CasdoorOptions
        {
            Domain = "https://sso.local",
            ClientId = "client-id",
            Secret = "client-secret",
            OrganizationName = Organization,
            AppName = "smartinsure",
            DefaultPassword = "senha-inicial",
            EnviromentUserCasdoor = "dev_insp",
        }));

    private void IdentityExists()
        => _api.GetUserAsync(ExternalIdentity, Arg.Any<CancellationToken>())
            .Returns(new CasdoorResponse<CasdoorUser?>
            {
                Status = "ok",
                Data = new CasdoorUser
                {
                    Id = ExternalIdentity,
                    Owner = Organization,
                    Name = Username,
                    NeedUpdatePassword = true,
                },
            });

    private void ProviderAnswers(string status, string? message = null)
        => _api.SetPasswordAsync(Arg.Any<Dictionary<string, string>>(), Arg.Any<CancellationToken>())
            .Returns(new CasdoorResponse<object> { Status = status, Msg = message });

    [Fact]
    public async Task SetPassword_DeveEnderecarIdentidadePorOrganizacaoEUsername_QuandoIdentidadeExiste()
    {
        IdentityExists();
        ProviderAnswers("ok");

        await _provider.SetPasswordAsync(ExternalIdentity, NewPassword, CancellationToken.None);

        // O `set-password` do Casdoor endereça por organização + username, não pelo UUID.
        await _api.Received(1).SetPasswordAsync(
            Arg.Is<Dictionary<string, string>>(form =>
                form["userOwner"] == Organization
                && form["userName"] == Username
                && form["newPassword"] == NewPassword),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SetPassword_DeveLancar_QuandoIdentidadeNaoExisteNoProvedor()
    {
        _api.GetUserAsync(ExternalIdentity, Arg.Any<CancellationToken>())
            .Returns(new CasdoorResponse<CasdoorUser?> { Status = "ok", Data = null });

        var act = () => _provider.SetPasswordAsync(
            ExternalIdentity, NewPassword, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*{ExternalIdentity}*");
        await _api.DidNotReceive().SetPasswordAsync(
            Arg.Any<Dictionary<string, string>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SetPassword_DeveLancar_QuandoProvedorRecusaAAtualizacao()
    {
        IdentityExists();
        ProviderAnswers("error", "password is not allowed");

        var act = () => _provider.SetPasswordAsync(
            ExternalIdentity, NewPassword, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*password is not allowed*");
    }
}

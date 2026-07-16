using System.IdentityModel.Tokens.Jwt;
using FluentAssertions;
using Microsoft.Extensions.Options;
using SmartInsure.Core.Entities;
using SmartInsure.Infra.CrossCutting.Identity;
using SmartInsure.Infra.CrossCutting.Options;

namespace SmartInsure.Tests.Infra.CrossCutting.Identity;

/// <summary>RN-005 — acesso autenticado emitido pela plataforma com validade de 8 horas.</summary>
[Trait("RuleId", "RN-005")]
public class JwtAccessTokenIssuerTests
{
    private readonly JwtAccessTokenIssuer _issuer = new(Options.Create(new JwtOptions
    {
        Issuer = "smartinsure",
        Audience = "smartinsure-web",
        SigningKey = "chave-simetrica-de-teste-com-tamanho-suficiente",
    }));

    [Fact]
    public void IssueFor_DeveEmitirAcessoValidoPor8Horas_QuandoUsuarioInformado()
    {
        var user = User.Create("Maria Silva", "maria@corretora.com.br", "casdoor-id-123");

        var accessToken = _issuer.IssueFor(user);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(accessToken.Token);

        jwt.Issuer.Should().Be("smartinsure");
        jwt.Audiences.Should().ContainSingle().Which.Should().Be("smartinsure-web");
        jwt.Subject.Should().Be("casdoor-id-123");
        (jwt.ValidTo - jwt.ValidFrom).Should().Be(TimeSpan.FromHours(8));
        accessToken.ExpiresAtUtc.Should().BeCloseTo(
            DateTime.UtcNow.AddHours(8), TimeSpan.FromMinutes(1));
    }
}

using FluentAssertions;
using SmartInsure.Integration.Casdoor.Services;

namespace SmartInsure.Tests.Integration.Casdoor.Services;

public class CasdoorIdentityProviderTests
{
    [Trait("RuleId", "RN-001")]
    [Theory]
    [InlineData("dev_insp", "maria.silva@onpoint.com.br", "dev_insp_maria_silva_onpoint_com_br")]
    [InlineData("dev_insp", "JOAO@ONPOINT.COM.BR", "dev_insp_joao_onpoint_com_br")]
    public void GetUsername_DeveSubstituirNaoAlfanumericoPorUnderline_QuandoEmailValido(
        string prefixo, string email, string esperado)
    {
        var username = CasdoorIdentityProvider.GetUsername(prefixo, email);

        username.Should().Be(esperado);
    }

    [Trait("RuleId", "RN-001")]
    [Fact]
    public void GetUsername_DeveLimitarEm39CaracteresSemUnderlineFinal_QuandoEmailLongo()
    {
        var username = CasdoorIdentityProvider.GetUsername(
            "dev_insp", "nome.extremamente.longo.de.usuario@onpoint.com.br");

        username.Length.Should().BeLessThanOrEqualTo(39);
        username.Should().NotEndWith("_");
        username.Should().MatchRegex("^[a-z0-9_]+$");
    }

    [Trait("RuleId", "RN-001")]
    [Fact]
    public void GetUsername_DeveTrocarUnderlineFinalPorZero_QuandoCorteTerminaEmUnderline()
    {
        // 30 chars antes do e-mail: "dev_insp_" (9) + 30 = 39; posição 39 cai no "_" pós-corte.
        var email = "abcdefghijklmnopqrstuvwxyzabc.d@x.com";

        var username = CasdoorIdentityProvider.GetUsername("dev_insp", email);

        username.Length.Should().Be(39);
        username.Should().EndWith("0");
    }
}

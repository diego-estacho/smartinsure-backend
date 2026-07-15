using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using SmartInsure.Core.Enumerators;
using SmartInsure.Integration.Bureau.Interfaces;
using SmartInsure.Integration.Bureau.Models;
using SmartInsure.Integration.Bureau.Services;

namespace SmartInsure.Tests.Integration.Bureau.Services;

public class BureauProviderTests
{
    private readonly IBureauApi _api = Substitute.For<IBureauApi>();
    private readonly BureauProvider _provider;

    public BureauProviderTests()
    {
        _provider = new BureauProvider(_api, NullLogger<BureauProvider>.Instance);
    }

    private static BureauPersonComplementResponse RespostaOk() => new()
    {
        Status = "OK",
        Cnpj = "17.901.209/0001-29",
        Nome = "FUNDO MUNICIPAL DE ASSISTENCIA SOCIAL",
        Situacao = "ATIVA",
        Uf = "SE",
        AtividadePrincipal = [new BureauActivity { Code = "88.00-6-00", Text = "Serviços de assistência social" }],
    };

    [Trait("RuleId", "RN-003")]
    [Fact]
    public async Task GetPersonComplementAsync_DeveRetornarDadosCadastrais_QuandoFonteRespondeOk()
    {
        _api.GetPersonComplementAsync(Arg.Any<BureauPersonComplementRequest>(), Arg.Any<CancellationToken>())
            .Returns(RespostaOk());

        var result = await _provider.GetPersonComplementAsync(
            "17901209000129", "Segurado", EBureau.ReceitaWS, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Name.Should().Be("FUNDO MUNICIPAL DE ASSISTENCIA SOCIAL");
        result.RegistrationStatus.Should().Be("ATIVA");
        result.MainActivities.Should().ContainSingle(a => a.Code == "88.00-6-00");
    }

    [Trait("RuleId", "RN-003")]
    [Fact]
    public async Task GetPersonComplementAsync_DeveConsultarFonteACadaSolicitacao_QuandoSolicitacoesIdenticas()
    {
        _api.GetPersonComplementAsync(Arg.Any<BureauPersonComplementRequest>(), Arg.Any<CancellationToken>())
            .Returns(RespostaOk());

        await _provider.GetPersonComplementAsync(
            "17901209000129", "Segurado", EBureau.ReceitaWS, CancellationToken.None);
        await _provider.GetPersonComplementAsync(
            "17901209000129", "Segurado", EBureau.ReceitaWS, CancellationToken.None);

        await _api.Received(2).GetPersonComplementAsync(
            Arg.Any<BureauPersonComplementRequest>(), Arg.Any<CancellationToken>());
    }

    [Trait("RuleId", "RN-003")]
    [Fact]
    public async Task GetPersonComplementAsync_DeveEnviarDocumentoTipoEBureauEscolhido_QuandoSolicitado()
    {
        BureauPersonComplementRequest? enviado = null;
        _api.GetPersonComplementAsync(
                Arg.Do<BureauPersonComplementRequest>(r => enviado = r), Arg.Any<CancellationToken>())
            .Returns(RespostaOk());

        await _provider.GetPersonComplementAsync(
            "17901209000129", "Segurado", EBureau.ReceitaWS, CancellationToken.None);

        enviado.Should().NotBeNull();
        enviado!.CpfCnpj.Should().Be("17901209000129");
        enviado.TipoPessoa.Should().Be("Segurado");
        enviado.BureauChoices.Should().Equal("ReceitaWS");
    }

    [Trait("RuleId", "RN-003")]
    [Theory]
    [InlineData("ERROR")]
    [InlineData("")]
    [InlineData(null)]
    public async Task GetPersonComplementAsync_DeveRetornarConsultaSemDado_QuandoStatusDiferenteDeOk(string? status)
    {
        _api.GetPersonComplementAsync(Arg.Any<BureauPersonComplementRequest>(), Arg.Any<CancellationToken>())
            .Returns(new BureauPersonComplementResponse { Status = status, Message = "não encontrado" });

        var result = await _provider.GetPersonComplementAsync(
            "17901209000129", "Segurado", EBureau.ReceitaWS, CancellationToken.None);

        result.Should().BeNull();
    }

    [Trait("RuleId", "RN-004")]
    [Fact]
    public async Task GetPersonComplementAsync_DeveRetornarConsultaSemDadoSemLancarExcecao_QuandoFonteIndisponivel()
    {
        _api.GetPersonComplementAsync(Arg.Any<BureauPersonComplementRequest>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("gateway indisponível"));

        var result = await _provider.GetPersonComplementAsync(
            "17901209000129", "Segurado", EBureau.ReceitaWS, CancellationToken.None);

        result.Should().BeNull();
    }

    [Trait("RuleId", "RN-004")]
    [Fact]
    public async Task GetPersonComplementAsync_DeveRetornarConsultaSemDado_QuandoTempoDeRespostaExcedido()
    {
        _api.GetPersonComplementAsync(Arg.Any<BureauPersonComplementRequest>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new TimeoutException("tempo de resposta excedido"));

        var result = await _provider.GetPersonComplementAsync(
            "17901209000129", "Segurado", EBureau.ReceitaWS, CancellationToken.None);

        result.Should().BeNull();
    }

    [Trait("RuleId", "RN-004")]
    [Fact]
    public async Task GetPersonComplementAsync_DevePropagarCancelamento_QuandoChamadorCancela()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        _api.GetPersonComplementAsync(Arg.Any<BureauPersonComplementRequest>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new OperationCanceledException(cts.Token));

        var act = () => _provider.GetPersonComplementAsync(
            "17901209000129", "Segurado", EBureau.ReceitaWS, cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Trait("RuleId", "RN-003")]
    [Fact]
    public void Map_DeveMapearTodosOsCamposCadastrais_QuandoRespostaCompleta()
    {
        var response = new BureauPersonComplementResponse
        {
            Status = "OK",
            Cnpj = "17.901.209/0001-29",
            Nome = "FUNDO MUNICIPAL",
            Fantasia = "FMAS",
            Situacao = "ATIVA",
            DataSituacao = "28/12/2012",
            Abertura = "28/12/2012",
            NaturezaJuridica = "133-3 - Fundo Público",
            Porte = "DEMAIS",
            CapitalSocial = "0.00",
            Logradouro = "R FREI LUIZ",
            Numero = "42",
            Complemento = "",
            Bairro = "PONTO NOVO",
            Municipio = "ARACAJU",
            Uf = "SE",
            Cep = "49.097-270",
            Telefone = "(79) 3218-7869",
            Email = "financeiro@aracaju.se.gov.br",
            AtividadePrincipal = [new BureauActivity { Code = "88.00-6-00", Text = "Assistência social" }],
            AtividadesSecundarias = [new BureauActivity { Code = "00.00-0-00", Text = "Não informada" }],
        };

        var dto = BureauProvider.Map(response);

        dto.Document.Should().Be("17.901.209/0001-29");
        dto.TradeName.Should().Be("FMAS");
        dto.RegistrationStatusDate.Should().Be("28/12/2012");
        dto.LegalNature.Should().Be("133-3 - Fundo Público");
        dto.City.Should().Be("ARACAJU");
        dto.ZipCode.Should().Be("49.097-270");
        dto.SecondaryActivities.Should().ContainSingle(a => a.Code == "00.00-0-00");
    }
}

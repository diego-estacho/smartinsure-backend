using FluentAssertions;
using NSubstitute;
using SmartInsure.Application.UseCase.Services.Quotations;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Repositories.Dtos;
using SmartInsure.Core.Enumerators;

namespace SmartInsure.Tests.Application.Services.Quotations;

/// <summary>
/// RN-105/RN-106 (ADR-103) — tradução das Coberturas Adicionais canônicas escolhidas para os NOMES
/// que a Seguradora reconhece, e a situação de cada uma.
/// </summary>
[Trait("RuleId", "RN-105")]
public sealed class QuotationAdditionalCoverageResolverTests
{
    private readonly IImportedAdditionalCoverageRepository _repository =
        Substitute.For<IImportedAdditionalCoverageRepository>();

    private static readonly Guid InsurerId = Guid.CreateVersion7();
    private static readonly Guid ModalityId = Guid.CreateVersion7();
    private static readonly Guid Multa = Guid.CreateVersion7();
    private static readonly Guid Trabalhista = Guid.CreateVersion7();

    [Fact]
    public async Task Resolve_DeveEnviarONomeDaImportada_QuandoNomeUnico_RN105()
    {
        var importedId = Guid.CreateVersion7();
        Arrange([new OfferableImportedCoverageDto(Multa, importedId, "Multas")]);

        var result = await Resolver().ResolveAsync(InsurerId, ModalityId, [Multa], CancellationToken.None);

        result.NamesToSend.Should().BeEquivalentTo(new[] { "Multas" });
        var item = result.Items.Should().ContainSingle().Subject;
        item.AdditionalCoverageId.Should().Be(Multa);
        item.Status.Should().Be(EQuotationAdditionalCoverageStatus.Sent);
        item.SentName.Should().Be("Multas");
        item.ImportedAdditionalCoverageId.Should().Be(importedId);
    }

    [Fact]
    public async Task Resolve_DeveEnviarUmaVez_QuandoOsRamosCompartilhamONome_RN105()
    {
        // Ramos Public e Private da mesma canônica com o MESMO nome (caso observado em QA na AXA):
        // o nome é inequívoco, envia uma vez; a Importada de origem fica indeterminada.
        Arrange(
        [
            new OfferableImportedCoverageDto(Multa, Guid.CreateVersion7(), "Multas"),
            new OfferableImportedCoverageDto(Multa, Guid.CreateVersion7(), "Multas"),
        ]);

        var result = await Resolver().ResolveAsync(InsurerId, ModalityId, [Multa], CancellationToken.None);

        result.NamesToSend.Should().BeEquivalentTo(new[] { "Multas" });
        var item = result.Items.Should().ContainSingle().Subject;
        item.Status.Should().Be(EQuotationAdditionalCoverageStatus.Sent);
        item.ImportedAdditionalCoverageId.Should().BeNull();
    }

    [Fact]
    [Trait("RuleId", "RN-106")]
    public async Task Resolve_DeveMarcarNaoContemplada_QuandoSeguradoraNaoOferece_RN106()
    {
        Arrange([]);

        var result = await Resolver().ResolveAsync(InsurerId, ModalityId, [Multa], CancellationToken.None);

        result.NamesToSend.Should().BeEmpty();
        var item = result.Items.Should().ContainSingle().Subject;
        item.Status.Should().Be(EQuotationAdditionalCoverageStatus.NotOffered);
        item.SentName.Should().BeNull();
        item.ImportedAdditionalCoverageId.Should().BeNull();
    }

    [Fact]
    [Trait("RuleId", "RN-106")]
    public async Task Resolve_DeveMarcarNaoContemplada_QuandoNomesDivergemEntreRamos_RN106()
    {
        // OPEN-22: sem regra de ramo não há como escolher. Enviar os dois derrubaria a Cotação
        // inteira (HTTP 400 do gateway — ADR-103), então falha para o lado seguro.
        Arrange(
        [
            new OfferableImportedCoverageDto(Multa, Guid.CreateVersion7(), "Multa"),
            new OfferableImportedCoverageDto(Multa, Guid.CreateVersion7(), "Multas"),
        ]);

        var result = await Resolver().ResolveAsync(InsurerId, ModalityId, [Multa], CancellationToken.None);

        result.NamesToSend.Should().BeEmpty();
        result.Items.Should().ContainSingle()
            .Which.Status.Should().Be(EQuotationAdditionalCoverageStatus.NotOffered);
    }

    [Fact]
    [Trait("RuleId", "RN-106")]
    public async Task Resolve_DeveResolverCadaCoberturaIndependentemente_RN106()
    {
        Arrange([new OfferableImportedCoverageDto(Multa, Guid.CreateVersion7(), "Multas")]);

        var result = await Resolver().ResolveAsync(
            InsurerId, ModalityId, [Multa, Trabalhista], CancellationToken.None);

        result.NamesToSend.Should().BeEquivalentTo(new[] { "Multas" });
        result.Items.Should().HaveCount(2);
        result.Items.Single(item => item.AdditionalCoverageId == Multa)
            .Status.Should().Be(EQuotationAdditionalCoverageStatus.Sent);
        result.Items.Single(item => item.AdditionalCoverageId == Trabalhista)
            .Status.Should().Be(EQuotationAdditionalCoverageStatus.NotOffered);
    }

    [Fact]
    public async Task Resolve_DeveEnviarNomeSemRepeticao_QuandoDuasCanonicasResolvemOMesmoNome_RN105()
    {
        // Curadoria pode vincular duas canônicas à mesma Importada; a Seguradora recebe o nome uma vez.
        Arrange(
        [
            new OfferableImportedCoverageDto(Multa, Guid.CreateVersion7(), "Multas"),
            new OfferableImportedCoverageDto(Trabalhista, Guid.CreateVersion7(), "Multas"),
        ]);

        var result = await Resolver().ResolveAsync(
            InsurerId, ModalityId, [Multa, Trabalhista], CancellationToken.None);

        result.NamesToSend.Should().BeEquivalentTo(new[] { "Multas" });
        result.Items.Should().OnlyContain(item => item.Status == EQuotationAdditionalCoverageStatus.Sent);
    }

    [Fact]
    public async Task Resolve_DeveDevolverVazio_QuandoGrupoNaoEscolheuCobertura_RN105()
    {
        var result = await Resolver().ResolveAsync(InsurerId, ModalityId, [], CancellationToken.None);

        result.NamesToSend.Should().BeEmpty();
        result.Items.Should().BeEmpty();
        await _repository.DidNotReceiveWithAnyArgs().ListForQuotationAsync(
            default, default, default!, default);
    }

    private void Arrange(IReadOnlyList<OfferableImportedCoverageDto> rows)
        => _repository.ListForQuotationAsync(
                Arg.Any<Guid>(),
                Arg.Any<Guid>(),
                Arg.Any<IReadOnlyCollection<Guid>>(),
                Arg.Any<CancellationToken>())
            .Returns(rows);

    private QuotationAdditionalCoverageResolver Resolver() => new(_repository);
}

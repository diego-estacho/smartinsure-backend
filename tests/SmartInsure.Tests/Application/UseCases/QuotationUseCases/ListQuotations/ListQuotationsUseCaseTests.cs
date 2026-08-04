using FluentAssertions;
using NSubstitute;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.ListQuotations;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.ListQuotations.Requests;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Tests.Application.UseCases.QuotationUseCases.ListQuotations;

/// <summary>
/// RN-057/RN-058 — Leitura do leque de Cotações do Grupo (acompanhamento por polling): o nº da proposta
/// exposto ao passo 4, o mesmo que a listagem usa como âncora (RN-077).
/// RN-106 — e a situação das Coberturas Adicionais de cada Cotação, para a comparação sinalizar o que
/// não foi contemplado.
/// </summary>
[Trait("RuleId", "RN-057")]
[Trait("RuleId", "RN-058")]
[Trait("RuleId", "RN-106")]
public class ListQuotationsUseCaseTests
{
    private readonly IQuotationGroupRepository _groupRepository = Substitute.For<IQuotationGroupRepository>();
    private readonly IQuotationRepository _quotationRepository = Substitute.For<IQuotationRepository>();
    private readonly IInsurerRepository _insurerRepository = Substitute.For<IInsurerRepository>();
    private readonly IAdditionalCoverageRepository _additionalCoverageRepository =
        Substitute.For<IAdditionalCoverageRepository>();

    private readonly ListQuotationsUseCase _useCase;

    public ListQuotationsUseCaseTests()
        => _useCase = new ListQuotationsUseCase(
            _groupRepository, _quotationRepository, _insurerRepository, _additionalCoverageRepository);

    private static QuotationGroup Group(IEnumerable<Guid>? additionalCoverageIds = null)
        => QuotationGroup.Create(
            Guid.CreateVersion7(), branchPersonId: null, Guid.CreateVersion7(), Guid.CreateVersion7(),
            100_000m, new DateOnly(2026, 8, 1), new DateOnly(2027, 8, 1),
            EQuotationScopeMode.All, [], additionalCoverageIds ?? []);

    /// <summary>Nomes/logos vazios e sem canônicas — o mínimo para o use case rodar.</summary>
    private void SetupLookups(IReadOnlyDictionary<Guid, string>? insurerNames = null)
    {
        IReadOnlyDictionary<Guid, string> empty = new Dictionary<Guid, string>();
        _insurerRepository.GetCorporateNamesByIdsAsync(
                Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(insurerNames ?? empty);
        _insurerRepository.GetLogoUrlsByIdsAsync(
                Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(empty);
        _additionalCoverageRepository.GetNamesByIdsAsync(
                Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(empty);
    }

    [Fact]
    public async Task Execute_DeveExporNumeroDaProposta_NaCotacaoObtida()
    {
        var group = Group();
        var insurerId = Guid.CreateVersion7();
        var quotation = Quotation.Requested(group.Id, insurerId);
        quotation.MarkObtained(
            EQuotationResult.ReadyForEmission, analysisTrack: null,
            premium: 190m, commissionPercentage: 25m, commissionValue: 47.5m, tax: 1m, availableLimit: 3_000_000m,
            proposalExternalId: "ext-1", proposalNumber: "202600000274285",
            requiresCcg: false, ccgMaxLimitWithoutNeed: null, ccgSigned: false,
            reasonTexts: [], obtainedAt: DateTime.UtcNow);

        _groupRepository.GetByIdAsync(group.Id, Arg.Any<CancellationToken>()).Returns(group);
        IReadOnlyList<Quotation> quotations = [quotation];
        _quotationRepository.ListByGroupAsync(group.Id, Arg.Any<CancellationToken>()).Returns(quotations);
        SetupLookups(new Dictionary<Guid, string> { [insurerId] = "Essor Seguros S.A." });

        var response = await _useCase.ExecuteAsync(new ListQuotationsRequest(group.Id), CancellationToken.None);

        response.Quotations.Should().ContainSingle();
        response.Quotations[0].Number.Should().Be("202600000274285");
    }

    [Fact]
    public async Task Execute_DeveTrazerNumeroNulo_QuandoAindaRequested()
    {
        var group = Group();
        var insurerId = Guid.CreateVersion7();
        // Requested (ainda cotando): a Seguradora não atribuiu proposta — número nulo.
        var quotation = Quotation.Requested(group.Id, insurerId);

        _groupRepository.GetByIdAsync(group.Id, Arg.Any<CancellationToken>()).Returns(group);
        IReadOnlyList<Quotation> quotations = [quotation];
        _quotationRepository.ListByGroupAsync(group.Id, Arg.Any<CancellationToken>()).Returns(quotations);
        SetupLookups();

        var response = await _useCase.ExecuteAsync(new ListQuotationsRequest(group.Id), CancellationToken.None);

        response.Quotations[0].Number.Should().BeNull();
    }

    [Fact]
    public async Task Execute_DeveFalhar_QuandoGrupoNaoEncontrado()
    {
        _groupRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((QuotationGroup?)null);

        var act = () => _useCase.ExecuteAsync(new ListQuotationsRequest(Guid.CreateVersion7()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Execute_DeveExporSituacaoDasCoberturas_RN106()
    {
        var multa = Guid.CreateVersion7();
        var trabalhista = Guid.CreateVersion7();
        var group = Group([multa, trabalhista]);

        var quotation = Quotation.Requested(group.Id, Guid.CreateVersion7());
        quotation.RecordAdditionalCoverages(
        [
            new ResolvedAdditionalCoverage(multa, EQuotationAdditionalCoverageStatus.Sent, "Multas", null),
            new ResolvedAdditionalCoverage(trabalhista, EQuotationAdditionalCoverageStatus.NotOffered, null, null),
        ]);

        _groupRepository.GetByIdAsync(group.Id, Arg.Any<CancellationToken>()).Returns(group);
        IReadOnlyList<Quotation> quotations = [quotation];
        _quotationRepository.ListByGroupAsync(group.Id, Arg.Any<CancellationToken>()).Returns(quotations);
        SetupLookups();
        IReadOnlyDictionary<Guid, string> coverageNames = new Dictionary<Guid, string>
        {
            [multa] = "Multas",
            [trabalhista] = "Trabalhista e Previdenciária",
        };
        _additionalCoverageRepository.GetNamesByIdsAsync(
                Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(coverageNames);

        var response = await _useCase.ExecuteAsync(
            new ListQuotationsRequest(group.Id), CancellationToken.None);

        var item = response.Quotations.Should().ContainSingle().Subject;
        item.AdditionalCoverages.Should().HaveCount(2);

        var enviada = item.AdditionalCoverages.Single(coverage => coverage.AdditionalCoverageId == multa);
        enviada.Status.Should().Be("Sent");
        // Nome apresentado é o da canônica; o nome de origem enviado fica em SentName.
        enviada.Name.Should().Be("Multas");
        enviada.SentName.Should().Be("Multas");

        var naoContemplada = item.AdditionalCoverages
            .Single(coverage => coverage.AdditionalCoverageId == trabalhista);
        naoContemplada.Status.Should().Be("NotOffered");
        naoContemplada.Name.Should().Be("Trabalhista e Previdenciária");
        naoContemplada.SentName.Should().BeNull();
    }

    [Fact]
    public async Task Execute_DeveDevolverListaVazia_QuandoGrupoNaoEscolheuCobertura_RN106()
    {
        var group = Group();
        var quotation = Quotation.Requested(group.Id, Guid.CreateVersion7());

        _groupRepository.GetByIdAsync(group.Id, Arg.Any<CancellationToken>()).Returns(group);
        IReadOnlyList<Quotation> quotations = [quotation];
        _quotationRepository.ListByGroupAsync(group.Id, Arg.Any<CancellationToken>()).Returns(quotations);
        SetupLookups();

        var response = await _useCase.ExecuteAsync(
            new ListQuotationsRequest(group.Id), CancellationToken.None);

        response.Quotations.Should().ContainSingle()
            .Which.AdditionalCoverages.Should().BeEmpty();
    }
}

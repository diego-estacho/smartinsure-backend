using FluentAssertions;
using NSubstitute;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.ListQuotations;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.ListQuotations.Requests;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;

namespace SmartInsure.Tests.Application.UseCases.QuotationUseCases.ListQuotations;

/// <summary>
/// RN-106 — a leitura do leque expõe a situação das Coberturas Adicionais de cada Cotação, para a
/// comparação sinalizar o que não foi contemplado.
/// </summary>
[Trait("RuleId", "RN-106")]
public sealed class ListQuotationsUseCaseTests
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

    [Fact]
    public async Task Execute_DeveExporSituacaoDasCoberturas_RN106()
    {
        var multa = Guid.CreateVersion7();
        var trabalhista = Guid.CreateVersion7();

        var group = QuotationGroup.Create(
            Guid.CreateVersion7(), null, Guid.CreateVersion7(), Guid.CreateVersion7(), 1_000m,
            new DateOnly(2026, 9, 1), new DateOnly(2026, 12, 1),
            EQuotationScopeMode.All, [], [multa, trabalhista]);

        var quotation = Quotation.Requested(group.Id, Guid.CreateVersion7());
        quotation.RecordAdditionalCoverages(
        [
            new ResolvedAdditionalCoverage(multa, EQuotationAdditionalCoverageStatus.Sent, "Multas", null),
            new ResolvedAdditionalCoverage(trabalhista, EQuotationAdditionalCoverageStatus.NotOffered, null, null),
        ]);

        _groupRepository.GetByIdAsync(group.Id, Arg.Any<CancellationToken>()).Returns(group);
        _quotationRepository.ListByGroupAsync(group.Id, Arg.Any<CancellationToken>()).Returns([quotation]);
        _insurerRepository.GetCorporateNamesByIdsAsync(
                Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, string>());
        _insurerRepository.GetLogoUrlsByIdsAsync(
                Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, string?>());
        _additionalCoverageRepository.GetNamesByIdsAsync(
                Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, string>
            {
                [multa] = "Multas",
                [trabalhista] = "Trabalhista e Previdenciária",
            });

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
        var group = QuotationGroup.Create(
            Guid.CreateVersion7(), null, Guid.CreateVersion7(), Guid.CreateVersion7(), 1_000m,
            new DateOnly(2026, 9, 1), new DateOnly(2026, 12, 1),
            EQuotationScopeMode.All, [], []);

        var quotation = Quotation.Requested(group.Id, Guid.CreateVersion7());

        _groupRepository.GetByIdAsync(group.Id, Arg.Any<CancellationToken>()).Returns(group);
        _quotationRepository.ListByGroupAsync(group.Id, Arg.Any<CancellationToken>()).Returns([quotation]);
        _insurerRepository.GetCorporateNamesByIdsAsync(
                Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, string>());
        _insurerRepository.GetLogoUrlsByIdsAsync(
                Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, string?>());
        _additionalCoverageRepository.GetNamesByIdsAsync(
                Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, string>());

        var response = await _useCase.ExecuteAsync(
            new ListQuotationsRequest(group.Id), CancellationToken.None);

        response.Quotations.Should().ContainSingle()
            .Which.AdditionalCoverages.Should().BeEmpty();
    }
}

using FluentAssertions;
using NSubstitute;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.ListQuotations;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.ListQuotations.Requests;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Tests.Application.UseCases.QuotationUseCases.ListQuotations;

/// <summary>
/// RN-057/RN-058 — Leitura do leque de Cotações do Grupo (acompanhamento por polling). Foca no nº da
/// proposta (ProposalNumber) exposto ao passo 4, o mesmo que a listagem usa como âncora (RN-077).
/// </summary>
[Trait("RuleId", "RN-057")]
[Trait("RuleId", "RN-058")]
public class ListQuotationsUseCaseTests
{
    private readonly IQuotationGroupRepository _groupRepository = Substitute.For<IQuotationGroupRepository>();
    private readonly IQuotationRepository _quotationRepository = Substitute.For<IQuotationRepository>();
    private readonly IInsurerRepository _insurerRepository = Substitute.For<IInsurerRepository>();
    private readonly ListQuotationsUseCase _useCase;

    public ListQuotationsUseCaseTests()
        => _useCase = new ListQuotationsUseCase(_groupRepository, _quotationRepository, _insurerRepository);

    private static QuotationGroup Group()
        => QuotationGroup.Create(
            Guid.CreateVersion7(), branchPersonId: null, Guid.CreateVersion7(), Guid.CreateVersion7(),
            100_000m, new DateOnly(2026, 8, 1), new DateOnly(2027, 8, 1),
            EQuotationScopeMode.All, [], includesPenaltyCoverage: false, includesLaborCoverage: false);

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
        IReadOnlyDictionary<Guid, string> names =
            new Dictionary<Guid, string> { [insurerId] = "Essor Seguros S.A." };
        IReadOnlyDictionary<Guid, string> logos = new Dictionary<Guid, string>();
        _insurerRepository.GetCorporateNamesByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(names);
        _insurerRepository.GetLogoUrlsByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(logos);

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
        IReadOnlyDictionary<Guid, string> empty = new Dictionary<Guid, string>();
        _insurerRepository.GetCorporateNamesByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(empty);
        _insurerRepository.GetLogoUrlsByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(empty);

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
}

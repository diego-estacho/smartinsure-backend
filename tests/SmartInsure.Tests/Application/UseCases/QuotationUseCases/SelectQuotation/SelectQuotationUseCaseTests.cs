using FluentAssertions;
using NSubstitute;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.SelectQuotation;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.SelectQuotation.Requests;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Tests.Application.UseCases.QuotationUseCases.SelectQuotation;

/// <summary>RN-059 — Seleção da Cotação para seguir: seguibilidade, posse e substituição da escolha.</summary>
[Trait("RuleId", "RN-059")]
public class SelectQuotationUseCaseTests
{
    private static readonly DateTime Obtained = new(2026, 7, 28, 12, 0, 0, DateTimeKind.Utc);

    private readonly IQuotationGroupRepository _groupRepository = Substitute.For<IQuotationGroupRepository>();
    private readonly IQuotationRepository _quotationRepository = Substitute.For<IQuotationRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly SelectQuotationUseCase _useCase;

    public SelectQuotationUseCaseTests()
        => _useCase = new SelectQuotationUseCase(_groupRepository, _quotationRepository, _unitOfWork);

    private static QuotationGroup Group()
        => QuotationGroup.Create(
            Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7(),
            100_000m, new DateOnly(2026, 8, 1), new DateOnly(2027, 8, 1),
            EQuotationScopeMode.All, [], includesPenaltyCoverage: false, includesLaborCoverage: false);

    private static Quotation Followable(Guid groupId)
    {
        var quotation = Quotation.Requested(groupId, Guid.CreateVersion7());
        quotation.MarkObtained(
            EQuotationResult.ReadyForEmission, analysisTrack: null,
            premium: 300m, commissionPercentage: null, commissionValue: null, tax: null, availableLimit: null,
            proposalExternalId: null, proposalNumber: null,
            requiresCcg: false, ccgMaxLimitWithoutNeed: null, ccgSigned: false,
            reasonTexts: [], obtainedAt: Obtained);
        return quotation;
    }

    private static Quotation NotFollowable(Guid groupId)
    {
        var quotation = Quotation.Requested(groupId, Guid.CreateVersion7());
        quotation.MarkObtained(
            EQuotationResult.Unavailable, analysisTrack: null,
            premium: null, commissionPercentage: null, commissionValue: null, tax: null, availableLimit: null,
            proposalExternalId: null, proposalNumber: null,
            requiresCcg: false, ccgMaxLimitWithoutNeed: null, ccgSigned: false,
            reasonTexts: ["Modalidade indisponível"], obtainedAt: Obtained);
        return quotation;
    }

    [Fact]
    public async Task Execute_DeveMarcarEscolhida_QuandoSeguivel()
    {
        var group = Group();
        var quotation = Followable(group.Id);
        _groupRepository.GetByIdAsync(group.Id, Arg.Any<CancellationToken>()).Returns(group);
        _quotationRepository.GetByIdAsync(quotation.Id, Arg.Any<CancellationToken>()).Returns(quotation);

        var response = await _useCase.ExecuteAsync(
            new SelectQuotationRequest(group.Id, quotation.Id), CancellationToken.None);

        group.SelectedQuotationId.Should().Be(quotation.Id);
        response.SelectedQuotationId.Should().Be(quotation.Id);
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_DeveRecusar_QuandoNaoSeguivel()
    {
        var group = Group();
        var quotation = NotFollowable(group.Id);
        _groupRepository.GetByIdAsync(group.Id, Arg.Any<CancellationToken>()).Returns(group);
        _quotationRepository.GetByIdAsync(quotation.Id, Arg.Any<CancellationToken>()).Returns(quotation);

        var act = () => _useCase.ExecuteAsync(
            new SelectQuotationRequest(group.Id, quotation.Id), CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
        group.SelectedQuotationId.Should().BeNull();
    }

    [Fact]
    public async Task Execute_DeveRecusar_QuandoCotacaoNaoPertenceAoGrupo()
    {
        var group = Group();
        var fromOtherGroup = Followable(Guid.CreateVersion7());
        _groupRepository.GetByIdAsync(group.Id, Arg.Any<CancellationToken>()).Returns(group);
        _quotationRepository.GetByIdAsync(fromOtherGroup.Id, Arg.Any<CancellationToken>()).Returns(fromOtherGroup);

        var act = () => _useCase.ExecuteAsync(
            new SelectQuotationRequest(group.Id, fromOtherGroup.Id), CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    public async Task Execute_DeveFalhar_QuandoGrupoNaoEncontrado()
    {
        _groupRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((QuotationGroup?)null);

        var act = () => _useCase.ExecuteAsync(
            new SelectQuotationRequest(Guid.CreateVersion7(), Guid.CreateVersion7()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Execute_DeveSubstituirEscolhaAnterior_QuandoSelecionaOutra()
    {
        var group = Group();
        var first = Followable(group.Id);
        var second = Followable(group.Id);
        group.SelectQuotation(first.Id);
        _groupRepository.GetByIdAsync(group.Id, Arg.Any<CancellationToken>()).Returns(group);
        _quotationRepository.GetByIdAsync(second.Id, Arg.Any<CancellationToken>()).Returns(second);

        await _useCase.ExecuteAsync(new SelectQuotationRequest(group.Id, second.Id), CancellationToken.None);

        group.SelectedQuotationId.Should().Be(second.Id);
    }
}

using FluentAssertions;
using NSubstitute;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.RunQuotations;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.RunQuotations.Requests;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Tests.Application.UseCases.QuotationUseCases.RunQuotations;

/// <summary>RN-056/RN-057/RN-060 — Solicitação de Cotações (fan-out request side): escopo, materialização e re-solicitação.</summary>
[Trait("RuleId", "RN-056")]
[Trait("RuleId", "RN-057")]
public class RunQuotationsUseCaseTests
{
    private readonly IQuotationGroupRepository _groupRepository = Substitute.For<IQuotationGroupRepository>();
    private readonly IBrokerageInsurerEnablementRepository _enablementRepository =
        Substitute.For<IBrokerageInsurerEnablementRepository>();
    private readonly IQuotationRepository _quotationRepository = Substitute.For<IQuotationRepository>();
    private readonly IQuotationRequestChannel _channel = Substitute.For<IQuotationRequestChannel>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly RunQuotationsUseCase _useCase;

    public RunQuotationsUseCaseTests()
        => _useCase = new RunQuotationsUseCase(
            _groupRepository, _enablementRepository, _quotationRepository, _channel, _unitOfWork);

    private static QuotationGroup GroupAll()
        => QuotationGroup.Create(
            Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7(),
            100_000m, new DateOnly(2026, 8, 1), new DateOnly(2027, 8, 1),
            EQuotationScopeMode.All, [], includesPenaltyCoverage: false, includesLaborCoverage: false);

    private static QuotationGroup GroupSpecific(IEnumerable<Guid> selectedInsurerIds)
        => QuotationGroup.Create(
            Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7(),
            100_000m, new DateOnly(2026, 8, 1), new DateOnly(2027, 8, 1),
            EQuotationScopeMode.Specific, selectedInsurerIds, includesPenaltyCoverage: false, includesLaborCoverage: false);

    private static BrokerageInsurerEnablement Enablement(Guid brokerageId, Guid insurerId)
        => BrokerageInsurerEnablement.Create(
            brokerageId, insurerId, ECalculationEngine.PlugV2, "{\"baseUrl\":\"https://x/\",\"key\":\"k\"}");

    private void SetupNoExistingQuotations(Guid groupId)
    {
        IReadOnlyList<Quotation> none = [];
        _quotationRepository.ListByGroupAsync(groupId, Arg.Any<CancellationToken>()).Returns(none);
        _quotationRepository.AddRangeAsync(Arg.Any<IEnumerable<Quotation>>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task Execute_DeveMaterializarEEnfileirarTodasHabilitadas_NoModoAll()
    {
        var brokerageId = Guid.CreateVersion7();
        var group = GroupAll();
        var insurerA = Guid.CreateVersion7();
        var insurerB = Guid.CreateVersion7();

        _groupRepository.GetByIdWithInsurersAsync(group.Id, Arg.Any<CancellationToken>()).Returns(group);
        IReadOnlyList<BrokerageInsurerEnablement> enablements =
            [Enablement(brokerageId, insurerA), Enablement(brokerageId, insurerB)];
        _enablementRepository.ListActiveByBrokerageAsync(brokerageId, Arg.Any<CancellationToken>()).Returns(enablements);
        SetupNoExistingQuotations(group.Id);

        List<Quotation>? persisted = null;
        _quotationRepository
            .AddRangeAsync(Arg.Do<IEnumerable<Quotation>>(q => persisted = q.ToList()), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var response = await _useCase.ExecuteAsync(new RunQuotationsRequest(group.Id, brokerageId), CancellationToken.None);

        response.RequestedCount.Should().Be(2);
        persisted.Should().HaveCount(2);
        persisted!.Should().OnlyContain(q => q.ProcessingStatus == EQuotationProcessingStatus.Requested);
        await _channel.Received(2).EnqueueAsync(Arg.Any<QuotationRequestWorkItem>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_DeveCotarSelecionadasEMarcarDemaisComoIndisponivelLocal_NoModoSpecific()
    {
        var brokerageId = Guid.CreateVersion7();
        var chosen = Guid.CreateVersion7();
        var other = Guid.CreateVersion7();
        var group = GroupSpecific([chosen]);

        _groupRepository.GetByIdWithInsurersAsync(group.Id, Arg.Any<CancellationToken>()).Returns(group);
        IReadOnlyList<BrokerageInsurerEnablement> enablements =
            [Enablement(brokerageId, chosen), Enablement(brokerageId, other)];
        _enablementRepository.ListActiveByBrokerageAsync(brokerageId, Arg.Any<CancellationToken>()).Returns(enablements);
        SetupNoExistingQuotations(group.Id);

        List<Quotation>? persisted = null;
        _quotationRepository
            .AddRangeAsync(Arg.Do<IEnumerable<Quotation>>(q => persisted = q.ToList()), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var response = await _useCase.ExecuteAsync(new RunQuotationsRequest(group.Id, brokerageId), CancellationToken.None);

        response.RequestedCount.Should().Be(1);
        persisted.Should().HaveCount(2);
        persisted!.Single(q => q.InsurerId == chosen).ProcessingStatus.Should().Be(EQuotationProcessingStatus.Requested);
        var local = persisted!.Single(q => q.InsurerId == other);
        local.Result.Should().Be(EQuotationResult.Unavailable);
        local.Reasons.First().Source.Should().Be(EQuotationReasonSource.Local);
        await _channel.Received(1).EnqueueAsync(Arg.Any<QuotationRequestWorkItem>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_DeveRecusar_QuandoSemHabilitacaoAtiva()
    {
        var brokerageId = Guid.CreateVersion7();
        var group = GroupAll();
        _groupRepository.GetByIdWithInsurersAsync(group.Id, Arg.Any<CancellationToken>()).Returns(group);
        IReadOnlyList<BrokerageInsurerEnablement> empty = [];
        _enablementRepository.ListActiveByBrokerageAsync(brokerageId, Arg.Any<CancellationToken>()).Returns(empty);

        var act = () => _useCase.ExecuteAsync(new RunQuotationsRequest(group.Id, brokerageId), CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    public async Task Execute_DeveRecusar_QuandoSpecificSemSelecionadas()
    {
        var brokerageId = Guid.CreateVersion7();
        var group = GroupSpecific([]);
        _groupRepository.GetByIdWithInsurersAsync(group.Id, Arg.Any<CancellationToken>()).Returns(group);
        IReadOnlyList<BrokerageInsurerEnablement> enablements = [Enablement(brokerageId, Guid.CreateVersion7())];
        _enablementRepository.ListActiveByBrokerageAsync(brokerageId, Arg.Any<CancellationToken>()).Returns(enablements);

        var act = () => _useCase.ExecuteAsync(new RunQuotationsRequest(group.Id, brokerageId), CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    public async Task Execute_DeveFalhar_QuandoGrupoNaoEncontrado()
    {
        _groupRepository.GetByIdWithInsurersAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((QuotationGroup?)null);

        var act = () => _useCase.ExecuteAsync(
            new RunQuotationsRequest(Guid.CreateVersion7(), Guid.CreateVersion7()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Execute_DeveSubstituirAnterioresEDescartarEscolha_AoResolicitar()
    {
        var brokerageId = Guid.CreateVersion7();
        var group = GroupAll();
        var insurerA = Guid.CreateVersion7();
        var previous = Quotation.Requested(group.Id, insurerA);
        group.SelectQuotation(previous.Id);

        _groupRepository.GetByIdWithInsurersAsync(group.Id, Arg.Any<CancellationToken>()).Returns(group);
        IReadOnlyList<BrokerageInsurerEnablement> enablements = [Enablement(brokerageId, insurerA)];
        _enablementRepository.ListActiveByBrokerageAsync(brokerageId, Arg.Any<CancellationToken>()).Returns(enablements);
        IReadOnlyList<Quotation> existing = [previous];
        _quotationRepository.ListByGroupAsync(group.Id, Arg.Any<CancellationToken>()).Returns(existing);
        _quotationRepository.AddRangeAsync(Arg.Any<IEnumerable<Quotation>>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        await _useCase.ExecuteAsync(new RunQuotationsRequest(group.Id, brokerageId), CancellationToken.None);

        group.SelectedQuotationId.Should().BeNull();
        _quotationRepository.Received(1).Remove(previous);
    }
}

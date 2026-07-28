using FluentAssertions;
using NSubstitute;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.RunQuotations;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.RunQuotations.Requests;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Channels;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Tests.Application.UseCases.QuotationUseCases.RunQuotations;

/// <summary>RN-056/RN-057/RN-060 — disparo do fan-out de cotação.</summary>
[Trait("RuleId", "RN-056")]
[Trait("RuleId", "RN-057")]
[Trait("RuleId", "RN-060")]
public class RunQuotationsUseCaseTests
{
    private static readonly Guid BrokerageId = Guid.CreateVersion7();

    private readonly IQuotationGroupRepository _groupRepository = Substitute.For<IQuotationGroupRepository>();
    private readonly IBrokerageInsurerEnablementRepository _enablementRepository =
        Substitute.For<IBrokerageInsurerEnablementRepository>();
    private readonly IQuotationRepository _quotationRepository = Substitute.For<IQuotationRepository>();
    private readonly IQuotationRequestChannel _channel = Substitute.For<IQuotationRequestChannel>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly RunQuotationsUseCase _useCase;

    public RunQuotationsUseCaseTests()
    {
        _useCase = new RunQuotationsUseCase(
            _groupRepository, _enablementRepository, _quotationRepository, _channel, _unitOfWork);
    }

    private static QuotationGroup NewGroup(EQuotationScopeMode scopeMode, IEnumerable<Guid> insurerIds)
        => QuotationGroup.Create(
            Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7(),
            1000m, new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31),
            scopeMode, insurerIds, includesPenaltyCoverage: false, includesLaborCoverage: false);

    [Fact]
    public async Task Execute_DeveRecusar_QuandoGrupoNaoEncontrado()
    {
        _groupRepository.GetByIdWithInsurersAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((QuotationGroup?)null);

        var act = () => _useCase.ExecuteAsync(new RunQuotationsRequest(Guid.CreateVersion7(), BrokerageId), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Execute_DeveRecusar_QuandoAllSemHabilitacaoAtiva()
    {
        var group = NewGroup(EQuotationScopeMode.All, []);
        _groupRepository.GetByIdWithInsurersAsync(group.Id, Arg.Any<CancellationToken>()).Returns(group);
        _enablementRepository.ListActiveByBrokerageAsync(BrokerageId, Arg.Any<CancellationToken>()).Returns([]);

        var act = () => _useCase.ExecuteAsync(new RunQuotationsRequest(group.Id, BrokerageId), CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>().WithMessage("*habilitadas*");
        await _quotationRepository.DidNotReceiveWithAnyArgs().AddRangeAsync(default!, default);
    }

    [Fact]
    public async Task Execute_DeveRecusar_QuandoSpecificVazio()
    {
        var group = NewGroup(EQuotationScopeMode.Specific, []);
        _groupRepository.GetByIdWithInsurersAsync(group.Id, Arg.Any<CancellationToken>()).Returns(group);

        var act = () => _useCase.ExecuteAsync(new RunQuotationsRequest(group.Id, BrokerageId), CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>().WithMessage("*escolhidas*");
    }

    [Fact]
    public async Task Execute_DeveCriarUmaCotacaoPorSeguradora_EEnfileirar_QuandoAll()
    {
        var group = NewGroup(EQuotationScopeMode.All, []);
        _groupRepository.GetByIdWithInsurersAsync(group.Id, Arg.Any<CancellationToken>()).Returns(group);

        var insurerA = Guid.CreateVersion7();
        var insurerB = Guid.CreateVersion7();
        _enablementRepository.ListActiveByBrokerageAsync(BrokerageId, Arg.Any<CancellationToken>())
            .Returns([
                BrokerageInsurerEnablement.Create(BrokerageId, insurerA, ECalculationEngine.PlugV2, "{}"),
                BrokerageInsurerEnablement.Create(BrokerageId, insurerB, ECalculationEngine.PlugV2, "{}"),
            ]);

        var response = await _useCase.ExecuteAsync(new RunQuotationsRequest(group.Id, BrokerageId), CancellationToken.None);

        response.RequestedCount.Should().Be(2);
        await _quotationRepository.Received(1).AddRangeAsync(
            Arg.Is<IEnumerable<Quotation>>(quotations => quotations.Count() == 2), Arg.Any<CancellationToken>());
        await _channel.Received(2).EnqueueAsync(Arg.Any<QuotationRequestWorkItem>(), Arg.Any<CancellationToken>());
        await _quotationRepository.Received(1).RemoveByGroupAsync(group.Id, Arg.Any<CancellationToken>());
    }
}

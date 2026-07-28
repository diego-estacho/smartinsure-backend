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

/// <summary>RN-059 — seleção de uma Cotação seguível para seguir.</summary>
[Trait("RuleId", "RN-059")]
public class SelectQuotationUseCaseTests
{
    private static readonly Guid BrokerageId = Guid.CreateVersion7();

    private readonly IQuotationRepository _quotationRepository = Substitute.For<IQuotationRepository>();
    private readonly IQuotationGroupRepository _groupRepository = Substitute.For<IQuotationGroupRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly SelectQuotationUseCase _useCase;

    public SelectQuotationUseCaseTests()
    {
        _useCase = new SelectQuotationUseCase(_quotationRepository, _groupRepository, _unitOfWork);
    }

    private static QuotationGroup NewGroup()
        => QuotationGroup.Create(
            Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7(),
            1000m, new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31),
            EQuotationScopeMode.All, [], false, false);

    private static Quotation Followable(Guid groupId)
    {
        var quotation = Quotation.Request(groupId, BrokerageId, Guid.CreateVersion7());
        quotation.MarkObtained(EQuotationResult.Automatic, null, 1m, null, null, null, null, null, null, false, null, false, []);
        return quotation;
    }

    [Fact]
    public async Task Execute_DeveMarcarEscolhida_QuandoSeguivelEDoGrupo()
    {
        var group = NewGroup();
        var quotation = Followable(group.Id);
        _quotationRepository.GetByIdAsync(quotation.Id, Arg.Any<CancellationToken>()).Returns(quotation);
        _groupRepository.GetByIdWithInsurersAsync(group.Id, Arg.Any<CancellationToken>()).Returns(group);

        var response = await _useCase.ExecuteAsync(new SelectQuotationRequest(group.Id, quotation.Id), CancellationToken.None);

        response.SelectedQuotationId.Should().Be(quotation.Id);
        group.SelectedQuotationId.Should().Be(quotation.Id);
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_DeveRecusar_QuandoNaoSeguivel()
    {
        var group = NewGroup();
        var quotation = Quotation.Request(group.Id, BrokerageId, Guid.CreateVersion7());
        quotation.MarkFailed("Indisponível");
        _quotationRepository.GetByIdAsync(quotation.Id, Arg.Any<CancellationToken>()).Returns(quotation);

        var act = () => _useCase.ExecuteAsync(new SelectQuotationRequest(group.Id, quotation.Id), CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>().WithMessage("*não pode ser escolhida*");
    }

    [Fact]
    public async Task Execute_DeveRecusar_QuandoCotacaoDeOutroGrupo()
    {
        var quotation = Followable(Guid.CreateVersion7());
        _quotationRepository.GetByIdAsync(quotation.Id, Arg.Any<CancellationToken>()).Returns(quotation);

        var act = () => _useCase.ExecuteAsync(new SelectQuotationRequest(Guid.CreateVersion7(), quotation.Id), CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>().WithMessage("*não pertence*");
    }

    [Fact]
    public async Task Execute_DeveRecusar_QuandoCotacaoNaoEncontrada()
    {
        _quotationRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Quotation?)null);

        var act = () => _useCase.ExecuteAsync(new SelectQuotationRequest(Guid.CreateVersion7(), Guid.CreateVersion7()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}

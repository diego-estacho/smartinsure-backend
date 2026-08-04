using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using SmartInsure.Application.UseCase.UseCases.IssuanceUseCases.UpdateQuotationTax;
using SmartInsure.Application.UseCase.UseCases.IssuanceUseCases.UpdateQuotationTax.Requests;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Tests.Application.UseCases.IssuanceUseCases;

/// <summary>
/// RN-504 — ajuste da taxa na emissão: a taxa nova é submetida à Seguradora, e prêmio, comissão e
/// opções de parcelamento devolvidos por ela passam a valer na Cotação escolhida. A plataforma não
/// calcula dinheiro (ADR-004) e não inventa limite de taxa — o veredito é da Seguradora (RN-511).
/// </summary>
[Trait("RuleId", "RN-504")]
public class UpdateQuotationTaxUseCaseTests
{
    private readonly IQuotationRepository _quotationRepository = Substitute.For<IQuotationRepository>();
    private readonly IQuotationGroupRepository _groupRepository = Substitute.For<IQuotationGroupRepository>();
    private readonly IBrokerageInsurerEnablementRepository _enablementRepository =
        Substitute.For<IBrokerageInsurerEnablementRepository>();
    private readonly IPersonRepository _personRepository = Substitute.For<IPersonRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICalculationEngine _engine = Substitute.For<ICalculationEngine>();

    private readonly Guid _brokerageId = Guid.CreateVersion7();
    private readonly Guid _insurerId = Guid.CreateVersion7();

    private QuotationGroup _group = null!;
    private Quotation _quotation = null!;

    private UpdateQuotationTaxUseCase BuildUseCase()
    {
        var services = new ServiceCollection();
        services.AddKeyedSingleton<ICalculationEngine>(ECalculationEngine.PlugV2, (_, _) => _engine);

        _group = QuotationGroup.Create(
            Guid.CreateVersion7(), null, Guid.CreateVersion7(), Guid.CreateVersion7(), 100_000m,
            new DateOnly(2026, 8, 1), new DateOnly(2027, 8, 1),
            EQuotationScopeMode.All, [], includesPenaltyCoverage: false, includesLaborCoverage: false);
        _group.AssignBrokerage(_brokerageId);

        var enablement = BrokerageInsurerEnablement.Create(
            _brokerageId, _insurerId, ECalculationEngine.PlugV2, "{\"baseUrl\":\"https://x/\",\"key\":\"k\"}");

        _quotation = Quotation.Requested(_group.Id, _insurerId);
        _quotation.MarkObtained(
            EQuotationResult.ReadyForEmission, null, 300m, 20m, 60m, 1.5m, 500_000m,
            "11111111-1111-1111-1111-111111111111", "PROP-1", false, null, false, [], DateTime.UtcNow);
        _quotation.SetEnablement(enablement.Id);
        _quotation.SetProviderOptions([new QuotationInstallmentOption { Number = 1, Value = 300m }], [0], []);

        _group.MarkQuoted();
        _group.SelectQuotation(_quotation.Id);

        _quotationRepository.GetByIdAsync(_quotation.Id, Arg.Any<CancellationToken>()).Returns(_quotation);
        _groupRepository.GetByIdAsync(_group.Id, Arg.Any<CancellationToken>()).Returns(_group);
        _enablementRepository.GetByIdAsync(enablement.Id, Arg.Any<CancellationToken>()).Returns(enablement);
        _personRepository.GetByIdAsync(_brokerageId, Arg.Any<CancellationToken>())
            .Returns(Person.Create("12345678000195", "Corretora", null, Guid.CreateVersion7()));

        return new UpdateQuotationTaxUseCase(
            _quotationRepository, _groupRepository, _enablementRepository, _personRepository,
            _unitOfWork, services.BuildServiceProvider());
    }

    private UpdateQuotationTaxRequest Request(decimal tax = 2.5m)
        => new() { QuotationGroupId = _group.Id, Tax = tax };

    [Fact]
    public async Task Execute_TaxaAceita_DeveSubstituirPremioComissaoEParcelamentoDaCotacaoEscolhida()
    {
        var useCase = BuildUseCase();
        _engine.UpdateProposalFinancialDataAsync(
                Arg.Any<string?>(), Arg.Any<UpdateProposalFinancialDataInput>(), Arg.Any<CancellationToken>())
            .Returns(new ProposalFinancialDataResult
            {
                Premium = 450m,
                Tax = 2.5m,
                CommissionPercentage = 25m,
                CommissionValue = 112.5m,
                InstallmentOptions = [new QuotationInstallmentOption { Number = 2, Value = 225m }],
                PossibleGracePeriodsInDays = [0, 30],
            });

        var response = await useCase.ExecuteAsync(Request(), CancellationToken.None);

        _quotation.Premium.Should().Be(450m);
        _quotation.Tax.Should().Be(2.5m);
        _quotation.CommissionPercentage.Should().Be(25m);
        _quotation.CommissionValue.Should().Be(112.5m);
        _quotation.HasInstallmentOption(2).Should().BeTrue();
        _quotation.HasInstallmentOption(1).Should().BeFalse("as opções antigas não sobrevivem ao recálculo");
        _quotation.HasGracePeriodOption(30).Should().BeTrue();
        response.Premium.Should().Be(450m);
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_RecusaDaSeguradora_DevePreservarOsValoresAnteriores()
    {
        var useCase = BuildUseCase();
        _engine.UpdateProposalFinancialDataAsync(
                Arg.Any<string?>(), Arg.Any<UpdateProposalFinancialDataInput>(), Arg.Any<CancellationToken>())
            .Throws(new CalculationEngineException("Taxa abaixo do prêmio mínimo da modalidade."));

        var act = () => useCase.ExecuteAsync(Request(0.01m), CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>().WithMessage("*prêmio mínimo*");
        _quotation.Premium.Should().Be(300m);
        _quotation.Tax.Should().Be(1.5m);
        await _unitOfWork.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Execute_TaxaSemFormatoValido_DeveRecusarSemAcionarASeguradora(decimal tax)
    {
        var useCase = BuildUseCase();

        var act = () => useCase.ExecuteAsync(Request(tax), CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
        await _engine.DidNotReceive().UpdateProposalFinancialDataAsync(
            Arg.Any<string?>(), Arg.Any<UpdateProposalFinancialDataInput>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_GrupoComEmissaoSolicitada_DeveRecusar()
    {
        var useCase = BuildUseCase();
        _group.MarkEmissionRequested();

        var act = () => useCase.ExecuteAsync(Request(), CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
        await _engine.DidNotReceive().UpdateProposalFinancialDataAsync(
            Arg.Any<string?>(), Arg.Any<UpdateProposalFinancialDataInput>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_SemCotacaoEscolhida_DeveRecusar()
    {
        var useCase = BuildUseCase();
        _group.ClearSelection();

        var act = () => useCase.ExecuteAsync(Request(), CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }
}

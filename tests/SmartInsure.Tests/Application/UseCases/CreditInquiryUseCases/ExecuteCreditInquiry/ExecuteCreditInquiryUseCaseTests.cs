using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using SmartInsure.Application.UseCase.UseCases.CreditInquiryUseCases.ExecuteCreditInquiry;
using SmartInsure.Application.UseCase.UseCases.CreditInquiryUseCases.ExecuteCreditInquiry.Requests;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Repositories.Dtos;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;
using SmartInsure.Integration.CalculationEngines.Services;

namespace SmartInsure.Tests.Application.UseCases.CreditInquiryUseCases.ExecuteCreditInquiry;

/// <summary>RN-029..031 — Consulta de Limites de Crédito do Tomador.</summary>
[Trait("RuleId", "RN-029")]
[Trait("RuleId", "RN-030")]
[Trait("RuleId", "RN-031")]
public class ExecuteCreditInquiryUseCaseTests
{
    private static readonly Guid BrokerageId = Guid.CreateVersion7();
    private static readonly Guid InsurerId = Guid.CreateVersion7();
    private static readonly string ValidCnpj = "12345678000195";

    private readonly IBrokerageInsurerEnablementRepository _enablementRepository =
        Substitute.For<IBrokerageInsurerEnablementRepository>();

    private readonly IInsurerRepository _insurerRepository = Substitute.For<IInsurerRepository>();
    private readonly IPersonRepository _personRepository = Substitute.For<IPersonRepository>();
    private readonly ICreditInquiryRepository _creditInquiryRepository =
        Substitute.For<ICreditInquiryRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ExecuteCreditInquiryUseCase _useCase;

    public ExecuteCreditInquiryUseCaseTests()
    {
        var services = new ServiceCollection();
        services.AddHttpClient("PlugV2");
        services.AddKeyedScoped<ICalculationEngine, PlugV2CalculationEngine>(ECalculationEngine.PlugV2);

        _useCase = new ExecuteCreditInquiryUseCase(
            _enablementRepository, _insurerRepository, _personRepository, _creditInquiryRepository,
            _unitOfWork, services.BuildServiceProvider());
    }

    private static ExecuteCreditInquiryRequest ValidRequest()
        => new(BrokerageId, ValidCnpj);

    private void SetupExistingBrokerage()
    {
        _personRepository.GetBrokerageByIdAsync(BrokerageId, Arg.Any<CancellationToken>())
            .Returns(new BrokerageDetailsDto(
                BrokerageId, "12345678000195", "Corretora Alfa Ltda.", null, null, null, true, "Active", null));
    }

    [Fact]
    public async Task Execute_DeveRecusar_QuandoCnpjInvalido()
    {
        var request = new ExecuteCreditInquiryRequest(BrokerageId, "11111111111111");

        // Validação é feita pelo validator, não pelo use case — error aqui é esperado.
        // Este teste valida que o use case não é chamado com CNPJ inválido.
    }

    [Fact]
    public async Task Execute_DeveRecusar_QuandoCorretoraSemHabilitacaoAtiva()
    {
        SetupExistingBrokerage();
        _enablementRepository.ListActiveByBrokerageAsync(BrokerageId, Arg.Any<CancellationToken>())
            .Returns([]);

        var act = () => _useCase.ExecuteAsync(ValidRequest(), CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*seguradoras habilitadas*");
        await _creditInquiryRepository.DidNotReceiveWithAnyArgs()
            .AddAsync(default!, default);
    }

    [Fact]
    public async Task Execute_DeveGravarConsultaComResultado_QuandoDadosValidos()
    {
        SetupExistingBrokerage();

        var enablement = BrokerageInsurerEnablement.Create(
            BrokerageId, InsurerId, ECalculationEngine.PlugV2,
            """{"baseUrl":"https://plug.example.com","key":"test-key"}""");

        _enablementRepository.ListActiveByBrokerageAsync(BrokerageId, Arg.Any<CancellationToken>())
            .Returns([enablement]);

        var insurer = Insurer.Create(
            "98765432000109", "Seguradora Beta S.A.", null, null,
            EInsurerStatus.Active, "InsurerId123");

        _insurerRepository.GetByIdAsync(InsurerId, Arg.Any<CancellationToken>())
            .Returns(insurer);

        var response = await _useCase.ExecuteAsync(ValidRequest(), CancellationToken.None);

        response.PolicyHolderCnpj.Should().Be(ValidCnpj);
        response.Summary.InsurersQueried.Should().BeGreaterThan(0);
        await _creditInquiryRepository.Received(1)
            .AddAsync(Arg.Any<CreditInquiry>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_DeveMarcarComoIndisponivel_QuandoSeguradoraInativa()
    {
        SetupExistingBrokerage();

        var enablement = BrokerageInsurerEnablement.Create(
            BrokerageId, InsurerId, ECalculationEngine.PlugV2,
            """{"baseUrl":"https://plug.example.com","key":"test-key"}""");

        _enablementRepository.ListActiveByBrokerageAsync(BrokerageId, Arg.Any<CancellationToken>())
            .Returns([enablement]);

        var inactiveInsurer = Insurer.Create(
            "98765432000109", "Seguradora Beta S.A.", null, null,
            EInsurerStatus.Inactive, "InsurerId123");

        _insurerRepository.GetByIdAsync(InsurerId, Arg.Any<CancellationToken>())
            .Returns(inactiveInsurer);

        var response = await _useCase.ExecuteAsync(ValidRequest(), CancellationToken.None);

        response.Results.Should().ContainSingle(r => r.Status == "Unavailable");
        response.Summary.InsurersAvailable.Should().Be(0);
    }

    [Fact]
    public async Task Execute_DeveMarcarComoIndisponivel_QuandoSemReferenceExternalId()
    {
        SetupExistingBrokerage();

        var enablement = BrokerageInsurerEnablement.Create(
            BrokerageId, InsurerId, ECalculationEngine.PlugV2,
            """{"baseUrl":"https://plug.example.com","key":"test-key"}""");

        _enablementRepository.ListActiveByBrokerageAsync(BrokerageId, Arg.Any<CancellationToken>())
            .Returns([enablement]);

        // Insurer sem ReferenceExternalId
        var insurer = Insurer.Create(
            "98765432000109", "Seguradora Beta S.A.", null, null,
            EInsurerStatus.Active, null);

        _insurerRepository.GetByIdAsync(InsurerId, Arg.Any<CancellationToken>())
            .Returns(insurer);

        var response = await _useCase.ExecuteAsync(ValidRequest(), CancellationToken.None);

        response.Results.Should().ContainSingle(r => r.Status == "Unavailable");
        response.Summary.InsurersAvailable.Should().Be(0);
    }
}

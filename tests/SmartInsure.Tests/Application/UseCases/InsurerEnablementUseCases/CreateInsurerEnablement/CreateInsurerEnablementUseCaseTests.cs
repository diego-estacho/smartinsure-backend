using FluentAssertions;
using NSubstitute;
using SmartInsure.Application.UseCase.UseCases.InsurerEnablementUseCases.CreateInsurerEnablement;
using SmartInsure.Application.UseCase.UseCases.InsurerEnablementUseCases.CreateInsurerEnablement.Requests;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Repositories.Dtos;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Tests.Application.UseCases.InsurerEnablementUseCases.CreateInsurerEnablement;

/// <summary>RN-022 — Habilitação de Seguradora para a Corretora.</summary>
[Trait("RuleId", "RN-022")]
public class CreateInsurerEnablementUseCaseTests
{
    private static readonly Guid BrokerageId = Guid.CreateVersion7();
    private static readonly Guid InsurerId = Guid.CreateVersion7();

    private readonly IInsurerEnablementRepository _enablementRepository =
        Substitute.For<IInsurerEnablementRepository>();

    private readonly IInsurerRepository _insurerRepository = Substitute.For<IInsurerRepository>();
    private readonly IPersonRepository _personRepository = Substitute.For<IPersonRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly CreateInsurerEnablementUseCase _useCase;

    public CreateInsurerEnablementUseCaseTests()
        => _useCase = new CreateInsurerEnablementUseCase(
            _enablementRepository, _insurerRepository, _personRepository, _unitOfWork);

    private static CreateInsurerEnablementRequest ValidRequest()
        => new(BrokerageId, InsurerId, "PlugV2", """{"brokerCnpj":"12345678000195"}""");

    private void SetupExistingBrokerageAndInsurer()
    {
        _personRepository.GetBrokerageByIdAsync(BrokerageId, Arg.Any<CancellationToken>())
            .Returns(new BrokerageDetailsDto(
                BrokerageId, "12345678000195", "Corretora Alfa Ltda.", null, null, null, true, "Active", null));

        _insurerRepository.GetByIdAsync(InsurerId, Arg.Any<CancellationToken>())
            .Returns(Insurer.Create(
                "98765432000109", "Seguradora Beta S.A.", null, null, EInsurerStatus.Active));
    }

    [Fact]
    public async Task Execute_DeveCriarHabilitacaoAtiva_QuandoDadosValidos()
    {
        SetupExistingBrokerageAndInsurer();
        _enablementRepository.PairExistsAsync(BrokerageId, InsurerId, Arg.Any<CancellationToken>())
            .Returns(false);

        var response = await _useCase.ExecuteAsync(ValidRequest(), CancellationToken.None);

        response.BrokerageId.Should().Be(BrokerageId);
        response.InsurerId.Should().Be(InsurerId);
        response.CalculationEngine.Should().Be("PlugV2");
        response.Status.Should().Be("Active");
        await _enablementRepository.Received(1)
            .AddAsync(Arg.Any<InsurerEnablement>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_DeveRecusar_QuandoParJaHabilitado()
    {
        SetupExistingBrokerageAndInsurer();
        _enablementRepository.PairExistsAsync(BrokerageId, InsurerId, Arg.Any<CancellationToken>())
            .Returns(true);

        var act = () => _useCase.ExecuteAsync(ValidRequest(), CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
        await _unitOfWork.DidNotReceiveWithAnyArgs().CommitAsync(default);
    }

    [Fact]
    public async Task Execute_DeveRecusar_QuandoCorretoraNaoEncontrada()
    {
        _personRepository.GetBrokerageByIdAsync(BrokerageId, Arg.Any<CancellationToken>())
            .Returns((BrokerageDetailsDto?)null);

        var act = () => _useCase.ExecuteAsync(ValidRequest(), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
        await _unitOfWork.DidNotReceiveWithAnyArgs().CommitAsync(default);
    }

    [Fact]
    public async Task Execute_DeveRecusar_QuandoSeguradoraNaoEncontrada()
    {
        _personRepository.GetBrokerageByIdAsync(BrokerageId, Arg.Any<CancellationToken>())
            .Returns(new BrokerageDetailsDto(
                BrokerageId, "12345678000195", "Corretora Alfa Ltda.", null, null, null, true, "Active", null));
        _insurerRepository.GetByIdAsync(InsurerId, Arg.Any<CancellationToken>())
            .Returns((Insurer?)null);

        var act = () => _useCase.ExecuteAsync(ValidRequest(), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
        await _unitOfWork.DidNotReceiveWithAnyArgs().CommitAsync(default);
    }

    [Fact]
    public async Task Execute_DeveRecusar_QuandoMotorDesconhecido()
    {
        var request = new CreateInsurerEnablementRequest(BrokerageId, InsurerId, "MotorInexistente", null);

        var act = () => _useCase.ExecuteAsync(request, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
        await _unitOfWork.DidNotReceiveWithAnyArgs().CommitAsync(default);
    }
}

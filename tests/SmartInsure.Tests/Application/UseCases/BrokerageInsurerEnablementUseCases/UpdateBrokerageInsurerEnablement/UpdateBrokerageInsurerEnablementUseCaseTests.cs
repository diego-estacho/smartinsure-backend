using FluentAssertions;
using NSubstitute;
using SmartInsure.Application.UseCase.UseCases.BrokerageInsurerEnablementUseCases.UpdateBrokerageInsurerEnablement;
using SmartInsure.Application.UseCase.UseCases.BrokerageInsurerEnablementUseCases.UpdateBrokerageInsurerEnablement.Requests;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Tests.Application.UseCases.BrokerageInsurerEnablementUseCases.UpdateBrokerageInsurerEnablement;

/// <summary>RN-022 — alteração do motor e dos parâmetros de conexão da Habilitação.</summary>
[Trait("RuleId", "RN-022")]
public class UpdateBrokerageInsurerEnablementUseCaseTests
{
    private readonly IBrokerageInsurerEnablementRepository _repository =
        Substitute.For<IBrokerageInsurerEnablementRepository>();

    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly UpdateBrokerageInsurerEnablementUseCase _useCase;

    public UpdateBrokerageInsurerEnablementUseCaseTests()
        => _useCase = new UpdateBrokerageInsurerEnablementUseCase(_repository, _unitOfWork);

    [Fact]
    public async Task Execute_DeveAtualizarParametrosDeConexao_QuandoHabilitacaoExiste()
    {
        var enablement = BrokerageInsurerEnablement.Create(
            Guid.CreateVersion7(), Guid.CreateVersion7(), ECalculationEngine.PlugV2, null);
        _repository.GetByIdAsync(enablement.Id, Arg.Any<CancellationToken>()).Returns(enablement);

        var response = await _useCase.ExecuteAsync(
            new UpdateBrokerageInsurerEnablementRequest(
                enablement.Id, "PlugV2", """{"brokerCnpj":"12345678000195"}"""),
            CancellationToken.None);

        response.ConnectionParameters.Should().Be("""{"brokerCnpj":"12345678000195"}""");
        response.CalculationEngine.Should().Be("PlugV2");
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_DeveRecusar_QuandoHabilitacaoNaoEncontrada()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((BrokerageInsurerEnablement?)null);

        var act = () => _useCase.ExecuteAsync(
            new UpdateBrokerageInsurerEnablementRequest(Guid.CreateVersion7(), "PlugV2", null),
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
        await _unitOfWork.DidNotReceiveWithAnyArgs().CommitAsync(default);
    }

    [Fact]
    public async Task Execute_DeveRecusar_QuandoMotorDesconhecido()
    {
        var act = () => _useCase.ExecuteAsync(
            new UpdateBrokerageInsurerEnablementRequest(Guid.CreateVersion7(), "MotorInexistente", null),
            CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
        await _unitOfWork.DidNotReceiveWithAnyArgs().CommitAsync(default);
    }
}
